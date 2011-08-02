namespace Sender
{
    using System;

    class Program
    {
        static int Main(string[] args)
        {
            RabbitProducer producer = new RabbitProducer();
            producer.Connect();
            producer.SendMessages();
            producer.Disconnect();

            Console.WriteLine("All sent.");
            return 0;
            //Console.ReadLine();
        }
    }
}
