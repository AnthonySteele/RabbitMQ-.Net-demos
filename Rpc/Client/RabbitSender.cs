namespace Client
{
    using System;
    using System.Threading;

    using Messages;

    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;

    class RabbitSender
    {
        private IConnection connection;
        private IModel channel;
        private string replyQueueName;
        private QueueingBasicConsumer replyConsumer;

        public void Connect()
        {
            ConnectionFactory factory = new ConnectionFactory
            {
                HostName = ConnectionConstants.HostName
            };

            connection = factory.CreateConnection();
            channel = connection.CreateModel();

            channel.QueueDeclare(ConnectionConstants.QueueName, false, false, false, null);
            MakeReplyConsumer();
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

        private void MakeReplyConsumer()
        {
            replyQueueName = this.channel.QueueDeclare();

            replyConsumer = new QueueingBasicConsumer(this.channel);
            this.channel.BasicConsume(replyQueueName, true, replyConsumer);
        }

        private const int MessageCount = 10;

        public void SendMessages()
        {
            WriteStartMessage();

            for (int index = 1; index <= MessageCount; index++)
            {
                this.SendRequest(index);
                Thread.Sleep(500);
            }
        }

        private static void WriteStartMessage()
        {
            string startMessage = string.Format("Sending {0} messages to {1}/{2}",
                MessageCount, ConnectionConstants.HostName, ConnectionConstants.QueueName);
            Console.WriteLine(startMessage);
        }


        private void SendRequest(int index)
        {
            RequestMessage message = new RequestMessage
            {
                Id = index,
                Request = string.Format("This is request {0}", index)
            };

            byte[] messageBody = message.ToByteArray();

            IBasicProperties requestProperties = channel.CreateBasicProperties();
            requestProperties.CorrelationId = Guid.NewGuid().ToString();
            requestProperties.ReplyTo = replyQueueName;

            this.channel.BasicPublish(string.Empty, ConnectionConstants.QueueName, requestProperties, messageBody);
            Console.WriteLine("Sent message #{0}", index);
            ReadReply();
        }

        private void ReadReply()
        {
            // this blocks
            BasicDeliverEventArgs replyInEnvelope = replyConsumer.Queue.Dequeue() as BasicDeliverEventArgs;
            if (replyInEnvelope != null)
            {
                object responseObject = SerializationHelper.FromByteArray(replyInEnvelope.Body);
                ReplyMessage responseMessage = responseObject as ReplyMessage;
                if (responseMessage != null)
                {
                    Console.WriteLine("Response: {0}", responseMessage); 
                }
            }
        }
    }
}
