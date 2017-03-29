using System.Threading;

namespace Receiver
{
    using System;
    using Messages;

    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;

    /// <summary>
    /// attach a queue to an exchange, 
    /// similar to the pub-sub example at http://www.rabbitmq.com/tutorials/tutorial-three-java.html
    /// </summary>
    internal class RabbitConsumer
    {
        private IConnection _connection;
        private IModel _channel;
        private EventingBasicConsumer _consumer;

        private const string ExchangeName = "PubSubTestExchange";

        private string _queueName;

        public void Connect()
        {
            ConnectionFactory factory = new ConnectionFactory
            {
                HostName = ConnectionConstants.HostName
            }; 
            
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(ExchangeName, "fanout");

            // queue name is generated
            _queueName = _channel.QueueDeclare();
            _channel.QueueBind(_queueName, ExchangeName, string.Empty);

            Console.WriteLine(" [*] Waiting for logs.");

            _consumer = new EventingBasicConsumer(_channel);
            _consumer.Received += Consumer_Received;
            _channel.BasicConsume(queue: _queueName,
                noAck: true,
                consumer: _consumer);
        }

        private void Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            try
            {
                var message = SerializationHelper.FromByteArray<SimpleMessage>(e.Body);
                Console.WriteLine("Received {0} : {1}", message.GetType().Name, message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed message: {0}", ex);
            }
        }

        public void ConsumeMessages()
        {
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

        private static void WriteStartMessage()
        {
            string startMessage =
                $"Waiting for messages on {ConnectionConstants.HostName}/{ConnectionConstants.QueueName}. Press 'q' to quit";
            Console.WriteLine(startMessage);
        }

        private EventingBasicConsumer MakeConsumer()
        {
            EventingBasicConsumer consumer = new EventingBasicConsumer(_channel);
            _channel.BasicConsume(_queueName, true, consumer);
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
    }
}