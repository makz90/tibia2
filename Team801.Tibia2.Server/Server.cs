using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Team801.Tibia2.Core.Networking.Models;
using Team801.Tibia2.Core.Networking.Packets;

namespace Team801.Tibia2.Server
{
    public class Server
    {
        // Network Stuff
        private readonly UdpClient _udpClient;
        public readonly int Port;

        // Messaging
        Thread _networkThread;

        private readonly ConcurrentQueue<NetworkMessage> _incomingMessages = new ConcurrentQueue<NetworkMessage>();
        private readonly ConcurrentQueue<Tuple<BasePacket, IPEndPoint>> _outgoingMessages = new ConcurrentQueue<Tuple<BasePacket, IPEndPoint>>();
        private readonly ConcurrentQueue<IPEndPoint> _sendByePacketTo = new ConcurrentQueue<IPEndPoint>();

        // Used to check if we are running the server or not
        private readonly ThreadSafe<bool> _running = new ThreadSafe<bool>();

        public Server(int port)
        {
            Port = port;

            // Create the UDP socket (IPv4)
            _udpClient = new UdpClient(Port, AddressFamily.InterNetwork);
        }

        // Notifies that we can start the server
        public void Start()
        {
            _running.Value = true;
        }

        // Starts a shutdown of the server
        public void Shutdown()
        {
            if (_running.Value)
            {
                Console.WriteLine("[Server] Shutdown requested by user.");

                // Close any active games

                // Stops the network thread
                _running.Value = false;
            }
        }

        // Cleans up any necessary resources
        public void Close()
        {
            _networkThread?.Join(TimeSpan.FromSeconds(10));
            _udpClient.Close();
        }

        // Main loop function for the server
        public void Run()
        {
            // Make sure we've called Start()
            if (_running.Value)
            {
                // Info
                Console.WriteLine("[Server] Running Game Server");

                // Start the packet receiving Thread
                _networkThread = new Thread(_networkRun);
                _networkThread.Start();

                // Startup the first Arena
                // _addNewArena();
            }

            // Main loop of game server
            bool running = _running.Value;
            while (running)
            {
                // If we have some messages in the queue, pull them out
                NetworkMessage nm;
                bool have = _incomingMessages.TryDequeue(out nm);
                if (have)
                {
                    // Depending on what type of packet it is process it
                    if (nm.Packet.Type == PacketType.RequestJoin)
                    {
                        // // We have a new client, put them into an arena
                        // bool added = _nextArena.TryAddPlayer(nm.Sender);
                        // if (added)
                        //     _playerToArenaMap.TryAdd(nm.Sender, _nextArena);
                        //
                        // // If they didn't go in that means we're full, make a new arena
                        // if (!added)
                        // {
                        //     _addNewArena();
                        //
                        //     // Now there should be room
                        //     _nextArena.TryAddPlayer(nm.Sender);
                        //     _playerToArenaMap.TryAdd(nm.Sender, _nextArena);
                        // }
                        //
                        // // Dispatch the message
                        // _nextArena.EnqueMessage(nm);
                    }
                    else
                    {
                        // // Dispatch it to an existing arena
                        // Arena arena;
                        // if (_playerToArenaMap.TryGetValue(nm.Sender, out arena))
                        //     arena.EnqueMessage(nm);
                    }
                }
                else
                    Thread.Sleep(1); // Take a short nap if there are no messages

                // Check for quit
                running &= _running.Value;
            }
        }

        #region Network Functions

        // This function is meant to be run in its own thread
        // Is writes and reads Packets to/from the UdpClient
        private void _networkRun()
        {
            if (!_running.Value)
                return;

            Console.WriteLine("[Server] Waiting for UDP datagrams on port {0}", Port);

            while (_running.Value)
            {
                bool canRead = _udpClient.Available > 0;
                int numToWrite = _outgoingMessages.Count;
                int numToDisconnect = _sendByePacketTo.Count;

                // Get data if there is some
                if (canRead)
                {
                    // Read in one datagram
                    IPEndPoint ep = new IPEndPoint(IPAddress.Any, 0);
                    byte[] data = _udpClient.Receive(ref ep); // Blocks

                    // Enque a new message
                    NetworkMessage nm = new NetworkMessage();
                    nm.Sender = ep;
                    nm.Packet = new BasePacket(data);
                    nm.ReceiveTime = DateTime.Now;

                    _incomingMessages.Enqueue(nm);

                    //Console.WriteLine("RCVD: {0}", nm.Packet);
                }

                // Write out queued
                for (int i = 0; i < numToWrite; i++)
                {
                    // Send some data
                    Tuple<BasePacket, IPEndPoint> msg;
                    bool have = _outgoingMessages.TryDequeue(out msg);
                    if (have)
                        msg.Item1.Send(_udpClient, msg.Item2);

                    //Console.WriteLine("SENT: {0}", msg.Item1);
                }

                // Notify clients of Bye
                for (int i = 0; i < numToDisconnect; i++)
                {
                    IPEndPoint to;
                    bool have = _sendByePacketTo.TryDequeue(out to);
                    if (have)
                    {
                        ByePacket bp = new ByePacket();
                        bp.Send(_udpClient, to);
                    }
                }

                // If Nothing happened, take a nap
                if (!canRead && (numToWrite == 0) && (numToDisconnect == 0))
                    Thread.Sleep(1);
            }

            Console.WriteLine("[Server] Done listening for UDP datagrams");

            // Wait for all arena's thread to join
            // Queue<Arena> arenas = new Queue<Arena>(_activeArenas.Keys);
            // if (arenas.Count > 0)
            // {
            //     Console.WriteLine("[Server] Waiting for active Areans to finish...");
            //     foreach (Arena arena in arenas)
            //         arena.JoinThread();
            // }

            // See which clients are left to notify of Bye
            if (_sendByePacketTo.Count > 0)
            {
                Console.WriteLine("[Server] Notifying remaining clients of shutdown...");

                // run in a loop until we've told everyone else
                IPEndPoint to;
                bool have = _sendByePacketTo.TryDequeue(out to);
                while (have)
                {
                    ByePacket bp = new ByePacket();
                    bp.Send(_udpClient, to);
                    have = _sendByePacketTo.TryDequeue(out to);
                }
            }
        }

        // Queues up a Packet to be send to another person
        public void SendPacket(BasePacket packet, IPEndPoint to)
        {
            _outgoingMessages.Enqueue(new Tuple<BasePacket, IPEndPoint>(packet, to));
        }

        // Will queue to send a ByePacket to the specified endpoint
        public void SendBye(IPEndPoint to)
        {
            _sendByePacketTo.Enqueue(to);
        }

        #endregion // Network Functions
    }
}