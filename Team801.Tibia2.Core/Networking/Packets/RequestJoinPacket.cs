namespace Team801.Tibia2.Core.Networking.Packets
{
    public class RequestJoinPacket : BasePacket
    {
        public RequestJoinPacket()
            : base(PacketType.RequestJoin)
        {
        }
    }
}
