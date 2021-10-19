using System;
using System.Net;
using Team801.Tibia2.Core.Networking.Packets;

namespace Team801.Tibia2.Core.Networking.Models
{
    public class NetworkMessage
    {
        public IPEndPoint Sender { get; set; }
        public BasePacket Packet { get; set; }
        public DateTime ReceiveTime { get; set; }
    }
}
