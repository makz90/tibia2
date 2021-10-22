using System;
using System.Net;
using System.Threading;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Team801.Tibia2.Client
{
    class Program
    {
        private static NetPeer _server;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello Client!");

            EventBasedNetListener listener = new EventBasedNetListener();
            NetManager client = new NetManager(listener);
            client.Start();
            client.Connect("localhost" /* host ip or name */, 9050 /* port */, "SomeConnectionKey" /* text key or NetDataWriter */);
            listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
            {
                Console.WriteLine($"We got: '{dataReader.GetString()}' from {fromPeer}");
                dataReader.Recycle();

                _server = fromPeer;
            };

            while (!Console.KeyAvailable)
            {
                client.PollEvents();
                Thread.Sleep(15);

                var test = Console.ReadLine();
                var writer = new NetDataWriter();
                writer.Put(test);
                _server?.Send(writer, DeliveryMethod.Unreliable);
            }

            client.Stop();
        }
    }
}
