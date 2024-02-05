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
public partial class QueryBlockPacket
{
    public WorldPosition QueryPosition { get; set; }
}

public class QueryBlockPacketFormatter : IPacketFormatter
{
    public static readonly Guuid PacketTypeId = Guuid.GetInternalGuuid("Net", "Packet", "QueryBlock");

    public object GetValue(Guuid _, ReadOnlySequence<byte> packet)
    {
        return MemoryPackSerializer.Deserialize<QueryBlockPacket>(packet)!;
    }

    public Memory<byte> ToPacket(Guuid _, object value)
    {
        Guard.IsNotNull(value);
        Guard.IsAssignableToType(value, typeof(QueryBlockPacket));
        return MemoryPackSerializer.Serialize((QueryBlockPacket)value);
    }
}