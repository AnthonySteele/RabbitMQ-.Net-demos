using System.Threading;

namespace Receiver
{
    using System;
    using Messages;

    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;

    internal class RabbitConsumer
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
            _consumer = new EventingBasicConsumer(_channel);
            _consumer.Received += _consumer_Received;
            _channel.BasicConsume(queue: ConnectionConstants.QueueName, noAck: true, consumer: _consumer);
        }

        private void _consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            try
            {
                object message = SerializationHelper.FromByteArray(e.Body);
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
    }
}