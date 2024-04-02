namespace Utopia.Core.Net;

public interface IPacketHandler
{
    public Task Handle(ConnectionContext context,Guuid packetId, object packet);
}
