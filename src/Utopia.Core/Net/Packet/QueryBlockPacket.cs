#region

using System.Buffers;
using CommunityToolkit.Diagnostics;
using MemoryPack;
using Utopia.Core.Map;

#endregion

namespace Utopia.Core.Net.Packet;

/// <summary>
///     查询地图包
/// </summary>
[MemoryPackable]
public partial class QueryBlockPacket : IWithPacketId
{
    public static Guuid PacketID => InternalHelper.NewInternalGuuid("Net", "Packet", "QueryBlock");

    public WorldPosition QueryPosition { get; set; }
}
