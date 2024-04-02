#region

using System.Buffers;
using CommunityToolkit.Diagnostics;
using MemoryPack;
using Utopia.Core.Map;

#endregion

namespace Utopia.Core.Net.Packet;

[MemoryPackable]
public partial class AreaInfomrationPacket
{
    public Guuid[] Entities { get; set; } = Array.Empty<Guuid>();

    public byte[][] EntityData { get; set; } = Array.Empty<byte[]>();

    public bool? Accessible { get; set; }

    public bool? Collidable { get; set; }

    public WorldPosition Position { get; set; }
}

public class AreaInfomrationPacketFormatter : IPacketFormatter
{
    public static readonly Guuid PacketTypeId = InternalHelper.NewInternalGuuid("Net", "Packet", "BlockInformation");

    public Guuid Id => PacketTypeId;

    public object GetValue(Guuid _, ReadOnlySequence<byte> packet)
    {
        return MemoryPackSerializer.Deserialize<AreaInfomrationPacket>(packet)!;
    }

    public Memory<byte> ToPacket(Guuid _, object value)
    {
        Guard.IsNotNull(value);
        Guard.IsAssignableToType(value, typeof(AreaInfomrationPacket));
        return MemoryPackSerializer.Serialize((AreaInfomrationPacket)value);
    }
}
