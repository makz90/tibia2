namespace Team801.Tibia2.Core.Networking.Packets
{
    public class AcceptJoinPacket : BasePacket
    {
        public AcceptJoinPacket()
            : base(PacketType.AcceptJoin)
        {
        }

        public AcceptJoinPacket(byte[] bytes)
            : base(bytes)
        {
        }
    }
}
