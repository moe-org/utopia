// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System.Collections.Concurrent;

namespace Utopia.Core.Net;

/// <summary>
/// 负责对包进行分发，是线程安全的。
/// </summary>
public interface IDispatcher
{
    ConcurrentDictionary<Guuid, IPacketHandler> Handlers { get; }

    /// <summary>
    /// if there is no handler for the packet,return false
    /// </summary>
    Task<bool> DispatchPacket(Guuid packetTypeId, object obj);
}

public class Dispatcher : IDispatcher
{
    public ConcurrentDictionary<Guuid, IPacketHandler> Handlers { get; } = new();

    public async Task<bool> DispatchPacket(Guuid packetTypeId, object obj)
    {
        if(Handlers.TryGetValue(packetTypeId,out var handler)){
            await handler.Handle(packetTypeId,obj);
            return true;
        }

        return false;
    }
}

