using System;

namespace Team801.Tibia2.Server
{
    class Program
    {
        private static Server _server;

        static void Main(string[] args)
        {
            // Setup the server
            const int port = 6000; //int.Parse(args[0].Trim());
            _server = new Server(port);

            // Add the Ctrl-C handler
            Console.CancelKeyPress += InterruptHandler;

            // Run it
            _server.Start();
            _server.Run();
            _server.Close();
        }

        private static void InterruptHandler(object sender, ConsoleCancelEventArgs args)
        {
            // Do a graceful shutdown
            args.Cancel = true;
            _server?.Shutdown();
        }
    }
}
