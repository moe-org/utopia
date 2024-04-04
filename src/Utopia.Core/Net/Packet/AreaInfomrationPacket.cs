#region

using System.Buffers;
using CommunityToolkit.Diagnostics;
using MemoryPack;
using Utopia.Core.Map;

#endregion

namespace Utopia.Core.Net.Packet;

[MemoryPackable]
public partial class AreaInfomrationPacket : IWithPacketId
{
    public static Guuid PacketID => InternalHelper.NewInternalGuuid("Net", "Packet", "BlockInformation");

    public Guuid[] Entities { get; set; } = Array.Empty<Guuid>();

    public byte[][] EntityData { get; set; } = Array.Empty<byte[]>();

    public bool? Accessible { get; set; }

    public bool? Collidable { get; set; }

    public WorldPosition Position { get; set; }
}
