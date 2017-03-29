namespace Sender
{
    using System;
    using System.Threading;

    using Messages;

    using RabbitMQ.Client;

    class RabbitProducer
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

        public void Disconnect()
        {
            _channel = null;

            if (_connection.IsOpen)
            {
                _connection.Close();
            }

            _connection.Dispose();
            _connection = null;
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
                    SendSimpleMessage(senderId, index);
                }
                else
                {
                    SendGuidMessage(senderId, index);
                }

                Thread.Sleep(500);
            }
        }

        private static void WriteStartMessage()
        {
            string startMessage =
                $"Sending {MessageCount} messages to {ConnectionConstants.HostName}/{ConnectionConstants.QueueName}";
            Console.WriteLine(startMessage);
        }

        private void SendGuidMessage(int senderId, int index)
        {
            GuidMessage message = new GuidMessage
                {
                    Identifier = Guid.NewGuid(),
                    Content = $"This is Guid message #{index} message from {senderId}"
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
            _channel.BasicPublish(string.Empty, ConnectionConstants.QueueName, null, messageBody);
        }
    }
}
