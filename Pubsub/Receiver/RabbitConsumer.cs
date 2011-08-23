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
        private IConnection connection;
        private IModel channel;

        private const string ExchangeName = "PubSubTestExchange";

        private string queueName;

        public void Connect()
        {
            ConnectionFactory factory = new ConnectionFactory
            {
                HostName = ConnectionConstants.HostName
            }; 
            
            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            channel.ExchangeDeclare(ExchangeName, "fanout");

            // queue name is generated
            queueName = channel.QueueDeclare();
            channel.QueueBind(queueName, ExchangeName, string.Empty);
        }

        public void ConsumeMessages()
        {
            QueueingBasicConsumer consumer = MakeConsumer();
            WriteStartMessage();

            bool done = false;
            while (! done)
            {
                ReadAMessage(consumer);

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
            channel.BasicConsume(queueName, true, consumer);
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