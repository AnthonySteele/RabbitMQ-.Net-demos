namespace Sender
{
    using System;
    using System.Threading;

    using Messages;

    using RabbitMQ.Client;

    class RabbitProducer
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

            channel.QueueDeclare(ConnectionConstants.PubSubQueueName, false, false, false, null);
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

        public void SendMessages()
        {  
            Random random = new Random();
            int senderId = random.Next(99999);

            for (int index = 1; index < 10; index++)
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

        private void SendGuidMessage(int senderId, int index)
        {
            GuidMessage message = new GuidMessage
                {
                    Identifier = Guid.NewGuid(),
                    Content = String.Format("This is Guid message #{0} message from {1}", index, senderId)
                };

            Console.WriteLine("Sent Guid message #{0} from sender {1}", index, senderId);
            SendMessage(message);
        }

        private void SendSimpleMessage(int senderId, int index)
        {
            SimpleMessage message = new SimpleMessage
                {
                    Id = index, 
                    Text = "This is simple message from " + senderId
                };

            Console.WriteLine("Sent simple message #{0} from sender {1}", index, senderId);
            SendMessage(message);
        }

        private void SendMessage<T>(T message)
        {
            byte[] messageBody = message.ToByteArray();
            channel.BasicPublish(string.Empty, ConnectionConstants.PubSubQueueName, null, messageBody);
        }
    }
}
