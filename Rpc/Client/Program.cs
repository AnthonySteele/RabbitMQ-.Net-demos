namespace Client
{
    using System;

    class Program
    {
        static void Main(string[] args)
        {
            RabbitSender sender = new RabbitSender();
            sender.Connect();
            sender.SendMessages();

            Console.WriteLine("Quitting...");
            sender.Disconnect();
        }
    }
}
