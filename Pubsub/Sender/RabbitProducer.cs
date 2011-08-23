namespace Sender
{
    using System;
    using System.Threading;

    using Messages;

    using RabbitMQ.Client;

    /// <summary>
    /// send messages to an exchange, 
    /// similar to the pub-sub example at http://www.rabbitmq.com/tutorials/tutorial-three-java.html
    /// </summary>
    class RabbitProducer
    {
        private IConnection connection;
        private IModel channel;

        private const string ExchangeName = "PubSubTestExchange";

        public void Connect()
        {
            ConnectionFactory factory = new ConnectionFactory
            {
                HostName = ConnectionConstants.HostName
            };

            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            channel.ExchangeDeclare(ExchangeName, "fanout");
        }

        public void Disconnect()
        {
            channel = null;

            if (connection.IsOpen)
            {
                connection.Close();
            }

            connection.Dispose();
            connection = null;
        }

        private const int MessageCount = 10;

        public void SendMessages()
        {
            WriteStartMessage(); 
            
            Random random = new Random();
            int senderId = random.Next(99999);

            for (int index = 1; index <= MessageCount; index++)
            {
                if (random.Next(2) == 1)
                {
                    this.SendSimpleMessage(senderId, index);
                }
                else
                {
                    this.SendGuidMessage(senderId, index);
                }

                Thread.Sleep(500);
            }
        }

        private static void WriteStartMessage()
        {
            string startMessage = string.Format("Sending {0} messages to {1}/{2}",
                MessageCount, ConnectionConstants.HostName, ConnectionConstants.QueueName);
            Console.WriteLine(startMessage);
        }

        private void SendGuidMessage(int senderId, int index)
        {
            GuidMessage message = new GuidMessage
                {
                    Identifier = Guid.NewGuid(),
                    Content = String.Format("This is Guid message #{0} message from {1}", index, senderId)
                };

            SendMessage(message);
            Console.WriteLine("Sent Guid message #{0} from sender {1}", index, senderId);
        }

        private void SendSimpleMessage(int senderId, int index)
        {
            SimpleMessage message = new SimpleMessage
                {
                    Id = index, 
                    Text = "This is simple message from " + senderId
                };

            SendMessage(message);
            Console.WriteLine("Sent simple message #{0} from sender {1}", index, senderId);
        }

        private void SendMessage<T>(T message)
        {
            byte[] messageBody = message.ToByteArray();
            channel.BasicPublish(ExchangeName, string.Empty, null, messageBody);
        }
    }
}
