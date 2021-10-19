using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Team801.Tibia2.Core.Networking.Packets
{
    public enum PacketType : uint
    {
        RequestJoin = 1,    // Client Request to join a game
        AcceptJoin,         // Server accepts join
        PlayerMove,          // Server tells Clients ball & paddle position, and scores
        Bye,                // Either Server or Client tells the other to end the connection
    }

    public abstract class BasePacket
    {
        // Packet Data
        public PacketType Type;
        public long Timestamp;                  // 64 bit timestamp from DateTime.Ticks 
        public byte[] Payload = new byte[0];

        #region Constructors
        // Creates a Packet with the set type and an empty Payload
        public BasePacket(PacketType type)
        {
            this.Type = type;
            Timestamp = DateTime.Now.Ticks;
        }

        // Creates a Packet from a byte array
        public BasePacket(byte[] bytes)
        {
            // Start peeling out the data from the byte array
            int i = 0;

            // Type
            this.Type = (PacketType)BitConverter.ToUInt32(bytes, 0);
            i += sizeof(PacketType);

            // Timestamp
            Timestamp = BitConverter.ToInt64(bytes, i);
            i += sizeof(long);

            // Rest is payload
            Payload = bytes.Skip(i).ToArray();
        }
        #endregion // Constructors

        // Gets the packet as a byte array
        public byte[] GetBytes()
        {
            int ptSize = sizeof(PacketType);
            int tsSize = sizeof(long);

            // Join the Packet data
            int i = 0;
            byte[] bytes = new byte[ptSize + tsSize + Payload.Length];

            // Type
            BitConverter.GetBytes((uint)this.Type).CopyTo(bytes, i);
            i += ptSize;

            // Timestamp
            BitConverter.GetBytes(Timestamp).CopyTo(bytes, i);
            i += tsSize;

            // Payload
            Payload.CopyTo(bytes, i);
            i += Payload.Length;

            return bytes;
        }

        public override string ToString()
        {
            return string.Format("[Packet:{0}\n  timestamp={1}\n  payload size={2}]",
                this.Type, new DateTime(Timestamp), Payload.Length);
        }

        // Sends a Packet to a specific receiver 
        public void Send(UdpClient client, IPEndPoint receiver)
        {
            // TODO maybe be async instead?
            byte[] bytes = GetBytes();
            client.Send(bytes, bytes.Length, receiver);
        }

        // Send a Packet to the default remote receiver (will throw error if not set)
        public void Send(UdpClient client)
        {
            byte[] bytes = GetBytes();
            client.Send(bytes, bytes.Length);
        }
    }
}
