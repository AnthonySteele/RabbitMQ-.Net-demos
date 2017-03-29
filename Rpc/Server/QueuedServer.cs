namespace Server
{
    using System;
    using Messages;

    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;

    internal class QueuedServer
    {
        private IConnection _connection;
        private IModel _channel;

        public void Connect()
        {
            ConnectionFactory factory = new ConnectionFactory
            {
                HostName = ConnectionConstants.HostName
            }; 
            
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(ConnectionConstants.QueueName, false, false, false, null);
        }

        public void ProcessMessages()
        {
            QueueingBasicConsumer consumer = MakeConsumer();
            WriteStartMessage();

            bool done = false;
            while (! done)
            {
                ProcessAMessage(consumer);

                done = WasQuitKeyPressed();
            }

            _connection.Close();
            _connection.Dispose();
            _connection = null;
        }

        private static void WriteStartMessage()
        {
            string startMessage =
                $"Waiting for messages on {ConnectionConstants.HostName}/{ConnectionConstants.QueueName}. Press 'q' to quit";
            Console.WriteLine(startMessage);
        }

        private QueueingBasicConsumer MakeConsumer()
        {
            QueueingBasicConsumer consumer = new QueueingBasicConsumer(_channel);
            _channel.BasicConsume(ConnectionConstants.QueueName, false, consumer);
            return consumer;
        }

        private bool WasQuitKeyPressed()
        {
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey();
                
                if (Char.ToUpperInvariant(keyInfo.KeyChar) == 'Q')
                {
                    return true;
                }
            }

            return false;
        }

        private void ProcessAMessage(QueueingBasicConsumer consumer)
        {
            BasicDeliverEventArgs messageInEnvelope = DequeueMessage(consumer);
            if (messageInEnvelope == null)
            {
                return;
            }

            try
            {
                object messageObject = SerializationHelper.FromByteArray(messageInEnvelope.Body);
                RequestMessage request = messageObject as RequestMessage;
                if (request != null)
                {
                    Console.WriteLine("Received message: {0}", request);

                    ReplyMessage replyMessage = MakeReply(request);

                    IBasicProperties requestProperties = messageInEnvelope.BasicProperties;
                    IBasicProperties responseProperties = consumer.Model.CreateBasicProperties();
                    responseProperties.CorrelationId = requestProperties.CorrelationId;
                    SendReply(requestProperties.ReplyTo, responseProperties, replyMessage);
                    _channel.BasicAck(messageInEnvelope.DeliveryTag, false);
 
                    Console.WriteLine("sent reply to: {0}", request);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed message: {0}", ex);
            }
        }

        private ReplyMessage MakeReply(RequestMessage message)
        {
            return new ReplyMessage
            {
                Id = message.Id,
                Reply = "Reply to " + message.Request
            };
        }

        private void SendReply(string replyQueueName, IBasicProperties responseProperties, ReplyMessage response)
        {
            _channel.BasicPublish(string.Empty, replyQueueName, responseProperties, response.ToByteArray());
        }

        private static BasicDeliverEventArgs DequeueMessage(QueueingBasicConsumer consumer)
        {
            const int timeoutMilseconds = 400;
            object result;

            consumer.Queue.Dequeue(timeoutMilseconds, out result);
            return result as BasicDeliverEventArgs;
        }
    }
}