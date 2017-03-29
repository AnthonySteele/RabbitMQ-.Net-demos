namespace Client
{
    using System;
    using System.Threading;

    using Messages;

    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;

    class RabbitSender
    {
        private IConnection _connection;
        private IModel _channel;
        private string _replyQueueName;
        private EventingBasicConsumer _replyConsumer;

        public void Connect()
        {
            ConnectionFactory factory = new ConnectionFactory
            {
                HostName = ConnectionConstants.HostName
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(ConnectionConstants.QueueName, false, false, false, null);
            MakeReplyConsumer();
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

        private void MakeReplyConsumer()
        {
            _replyQueueName = _channel.QueueDeclare();

            _replyConsumer = new EventingBasicConsumer(_channel);
            _channel.BasicConsume(_replyQueueName, true, _replyConsumer);
            _replyConsumer.Received += _replyConsumer_Received;
        }

        private const int MessageCount = 10;

        public void SendMessages()
        {
            WriteStartMessage();

            for (int index = 1; index <= MessageCount; index++)
            {
                SendRequest(index);
                Thread.Sleep(500);
            }
        }

        private static void WriteStartMessage()
        {
            string startMessage =
                $"Sending {MessageCount} messages to {ConnectionConstants.HostName}/{ConnectionConstants.QueueName}";
            Console.WriteLine(startMessage);
        }

        private void SendRequest(int index)
        {
            RequestMessage message = new RequestMessage
            {
                Id = index,
                Request = $"This is request {index}"
            };

            byte[] messageBody = message.ToByteArray();

            IBasicProperties requestProperties = _channel.CreateBasicProperties();
            requestProperties.CorrelationId = Guid.NewGuid().ToString();
            requestProperties.ReplyTo = _replyQueueName;

            _channel.BasicPublish(string.Empty, ConnectionConstants.QueueName, requestProperties, messageBody);
            Console.WriteLine("Sent message #{0}", index);
        }

        private void _replyConsumer_Received(object sender, BasicDeliverEventArgs e)
        {
            ReplyMessage responseMessage = SerializationHelper.FromByteArray<ReplyMessage>(e.Body);
            if (responseMessage != null)
            {
                Console.WriteLine("Response: {0}", responseMessage);
            }
        }
    }
}
