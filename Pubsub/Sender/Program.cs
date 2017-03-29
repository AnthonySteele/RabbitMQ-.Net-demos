namespace Sender
{
    using System;

    class Program
    {
        static void Main(string[] args)
        {
            RabbitProducer producer = new RabbitProducer();
            producer.Connect();
            producer.SendMessages();
            producer.Disconnect();

            Console.WriteLine("All sent.");
        }
    }
}
