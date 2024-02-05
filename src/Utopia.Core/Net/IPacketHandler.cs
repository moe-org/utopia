namespace Utopia.Core.Net;

public interface IPacketHandler
{
    public Task Handle(Guuid packetId, object packet);
}