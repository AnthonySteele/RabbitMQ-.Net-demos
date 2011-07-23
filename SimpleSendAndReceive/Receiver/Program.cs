namespace Receiver
{
    using System;

    class Program
    {
        static void Main(string[] args)
        {
            RabbitConsumer producer = new RabbitConsumer();
            producer.Connect();
            producer.ConsumeMessages();

            Console.WriteLine("Quitting...");
        }
    }
}
