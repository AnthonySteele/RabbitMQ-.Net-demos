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
        }

        public void ConsumeMessages()
        {
            QueueingBasicConsumer consumer = MakeConsumer();
            WriteStartMessage();

            bool done = false;
            while (! done)
            {
                ReadAMessage(consumer);

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

        private static void ReadAMessage(QueueingBasicConsumer consumer)
        {
            BasicDeliverEventArgs delivery = DequeueMessage(consumer);
            if (delivery == null)
            {
                return;
            }

            try
            {
                object message = SerializationHelper.FromByteArray(delivery.Body);
                Console.WriteLine("Received {0} : {1}", message.GetType().Name, message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed message: {0}", ex);
            }
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