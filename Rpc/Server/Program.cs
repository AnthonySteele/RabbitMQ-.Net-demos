namespace Server
{
    using System;

    class Program
    {
        static int Main(string[] args)
        {
            QueuedServer server = new QueuedServer();
            server.Connect();
            server.ProcessMessages();

            Console.WriteLine("Quitting...");
            return 0;
        }
    }
}
