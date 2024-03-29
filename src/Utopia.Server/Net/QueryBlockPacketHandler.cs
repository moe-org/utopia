// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System.Collections.Concurrent;
using Utopia.Core;
using Utopia.Core.Net;
using Utopia.Core.Net.Packet;
using Utopia.Server.Entity;
using Utopia.Server.Map;

namespace Utopia.Server.Net;

public class QueryBlockPacketHandler : IPacketHandler
{
    public required IConnectionHandler ConnectHandler { private get; init; }

    public required ConcurrentDictionary<Guuid,IWorld> Worlds { private get; init; }

    public Task Handle(Guuid packetId, object packet)
    {
        var query = (QueryBlockPacket)packet;

        if (!Worlds.TryGetValue(query.QueryPosition.Id, out IWorld? world))
        {
            return Task.FromResult(0);
        }

        if (world!.TryGetBlock(query.QueryPosition.ToPos(), out IBlock? block))
        {
            // using var _ = block!.EnterReadLock();

            var info = new AreaInfomrationPacket();
            IReadOnlyCollection<IEntity> entities = block!.GetAllEntities();
            info.Collidable = block.HasCollision;
            info.Accessible = block.Accessible;
            info.Position = query.QueryPosition;
            info.Entities = entities.Select((i) => i.Id).ToArray();
            info.EntityData = entities.Select((i) => i.SaveAs()).ToArray();

            ConnectHandler.WritePacket(BlockInfoPacketFormatter.PacketTypeId, info);
        }

        return Task.FromResult(0);
    }
}
