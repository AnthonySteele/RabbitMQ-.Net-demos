using System.Threading;

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
        private EventingBasicConsumer _consumer;

        public void Connect()
        {
            ConnectionFactory factory = new ConnectionFactory
            {
                HostName = ConnectionConstants.HostName
            }; 
            
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(ConnectionConstants.QueueName, false, false, false, null);
            _channel.BasicQos(0, 1, false);

            _consumer = new EventingBasicConsumer(_channel);
            _channel.BasicConsume(queue: ConnectionConstants.QueueName,
                noAck: false, consumer: _consumer);
            Console.WriteLine(" [x] Awaiting RPC requests");
            _consumer.Received += Consumer_Received;
        }

        public void ProcessMessages()
        {
            EventingBasicConsumer consumer = MakeConsumer();
            consumer.Received += Consumer_Received;
            WriteStartMessage();

            bool done = false;
            while (! done)
            {
                Thread.Sleep(1000);
                Console.Write(".");
                done = WasQuitKeyPressed();
            }

            _connection.Close();
            _connection.Dispose();
            _connection = null;
        }

        private void Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            try
            {
                RequestMessage request = SerializationHelper.FromByteArray<RequestMessage>(e.Body);
                if (request != null)
                {
                    Console.WriteLine("Received message: {0}", request);

                    ReplyMessage replyMessage = MakeReply(request);

                    IBasicProperties requestProperties = e.BasicProperties;
                    IBasicProperties responseProperties = _consumer.Model.CreateBasicProperties();
                    responseProperties.CorrelationId = requestProperties.CorrelationId;
                    SendReply(requestProperties.ReplyTo, responseProperties, replyMessage);
                    _channel.BasicAck(e.DeliveryTag, false);

                    Console.WriteLine("sent reply to: {0}", request);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed message: {0}", ex);
            }
        }

        private static void WriteStartMessage()
        {
            string startMessage =
                $"Waiting for messages on {ConnectionConstants.HostName}/{ConnectionConstants.QueueName}. Press 'q' to quit";
            Console.WriteLine(startMessage);
        }

        private EventingBasicConsumer MakeConsumer()
        {
            EventingBasicConsumer consumer = new EventingBasicConsumer(_channel);
            _channel.BasicConsume(ConnectionConstants.QueueName, false, consumer);
            return consumer;
        }

        private bool WasQuitKeyPressed()
        {
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey();
                
                if (char.ToUpperInvariant(keyInfo.KeyChar) == 'Q')
                {
                    return true;
                }
            }

            return false;
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
    }
}