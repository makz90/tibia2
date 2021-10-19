using System;

namespace Team801.Tibia2.Core.Networking.Packets
{
    public class PlayerMovePacket : BasePacket
    {
        // The Paddle's Y position
        public float Y
        {
            get { return BitConverter.ToSingle(Payload, 0); }
            set { BitConverter.GetBytes(value).CopyTo(Payload, 0); }
        }

        public float X
        {
            get { return BitConverter.ToSingle(Payload, 0); }
            set { BitConverter.GetBytes(value).CopyTo(Payload, 0); }
        }

        public PlayerMovePacket()
            : base(PacketType.PlayerMove)
        {
            Payload = new byte[sizeof(float)];

            // Default value is zero
            X = 0;
            Y = 0;
        }

        public PlayerMovePacket(byte[] bytes)
            : base(bytes)
        {
        }

        public override string ToString()
        {
            return $"[Packet:{this.Type}\n  timestamp={new DateTime(Timestamp)}\n  payload size={Payload.Length} \n X={this.X} Y={this.Y}]";
        }
    }
}
