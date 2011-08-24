namespace Server
{
    using System;
    using Messages;

    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;

    internal class QueuedServer
    {
        private IConnection connection;
        private IModel channel;

        public void Connect()
        {
            ConnectionFactory factory = new ConnectionFactory
            {
                HostName = ConnectionConstants.HostName
            }; 
            
            connection = factory.CreateConnection();
            channel = connection.CreateModel();

            channel.QueueDeclare(ConnectionConstants.QueueName, false, false, false, null);
        }

        public void ProcessMessages()
        {
            QueueingBasicConsumer consumer = MakeConsumer();
            WriteStartMessage();

            bool done = false;
            while (! done)
            {
                ProcessAMessage(consumer);

                done = this.WasQuitKeyPressed();
            }

            connection.Close();
            connection.Dispose();
            connection = null;
        }

        private static void WriteStartMessage()
        {
            string startMessage = string.Format("Waiting for messages on {0}/{1}. Press 'q' to quit",
                ConnectionConstants.HostName, ConnectionConstants.QueueName);
            Console.WriteLine(startMessage);
        }

        private QueueingBasicConsumer MakeConsumer()
        {
            QueueingBasicConsumer consumer = new QueueingBasicConsumer(channel);
            channel.BasicConsume(ConnectionConstants.QueueName, false, consumer);
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

                    ReplyMessage replyMessage = this.MakeReply(request);

                    IBasicProperties requestProperties = messageInEnvelope.BasicProperties;
                    IBasicProperties responseProperties = consumer.Model.CreateBasicProperties();
                    responseProperties.CorrelationId = requestProperties.CorrelationId;
                    this.SendReply(requestProperties.ReplyTo, responseProperties, replyMessage);
                    this.channel.BasicAck(messageInEnvelope.DeliveryTag, false);
 
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
            this.channel.BasicPublish(string.Empty, replyQueueName, responseProperties, response.ToByteArray());
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