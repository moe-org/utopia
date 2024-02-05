#region

using System.Collections.Concurrent;

#endregion

namespace Utopia.Core.Net;

/// <summary>
///     负责对包进行分发，是线程安全的。
/// </summary>
public interface IDispatcher
{
    ConcurrentDictionary<Guuid, IPacketHandler> Handlers { get; }

    /// <summary>
    ///     if there is no handler for the packet,return false
    /// </summary>
    Task<bool> DispatchPacket(Guuid packetTypeId, object obj);
}

public class Dispatcher : IDispatcher
{
    public ConcurrentDictionary<Guuid, IPacketHandler> Handlers { get; } = new();

    public async Task<bool> DispatchPacket(Guuid packetTypeId, object obj)
    {
        if (this.Handlers.TryGetValue(packetTypeId, out var handler))
        {
            await handler.Handle(packetTypeId, obj);
            return true;
        }

        return false;
    }
}