using CommunityToolkit.Diagnostics;
using Utopia.Core.Net.Packet;

namespace Utopia.Core.Net;

public interface IPacketHandler
{
    public Task Handle(ConnectionContext context,Guuid packetId, object packet);
}

public interface IPacketHandler<T> : IPacketHandler where T : IWithPacketId
{
    Task IPacketHandler.Handle(ConnectionContext context,Guuid pakcetId,object packet)
    {
        if(pakcetId != T.PacketID)
        {
            throw new ArgumentException($"this formatter can only process `{T.PacketID}` packet but get a `{pakcetId}` packet");
        }

        Guard.IsAssignableToType<T>(pakcetId);

        return Handle(context,(T)packet);
    }

    public Task Handle(ConnectionContext ctx, T packet);
}

