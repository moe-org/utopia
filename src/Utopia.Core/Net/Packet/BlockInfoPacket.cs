// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System.Buffers;
using CommunityToolkit.Diagnostics;
using MessagePack;
using Utopia.Core.Map;

namespace Utopia.Core.Net.Packet;
public class BlockInfoPacket
{
    [Key(0)]
    public Guuid[] Entities { get; set; } = Array.Empty<Guuid>();

    [Key(1)]
    public byte[][] EntityData { get; set; } = Array.Empty<byte[]>();

    [Key(2)]
    public bool? Accessible { get; set; }

    [Key(3)]
    public bool? Collidable { get; set; }

    [Key(4)]
    public WorldPosition Position { get; set; }
}

public class BlockInfoPacketFormatter : IPacketFormatter
{
    public static readonly Guuid PacketTypeId = Guuid.GetInternalGuuid("Net", "Packet", "BlockInformation");

    public Guuid Id => PacketTypeId;

    public object GetValue(Guuid _, ReadOnlySequence<byte> packet) => MessagePackSerializer.Deserialize<BlockInfoPacket>(packet);

    public Memory<byte> ToPacket(Guuid _, object value)
    {
        Guard.IsNotNull(value);
        Guard.IsAssignableToType(value, typeof(BlockInfoPacket));
        return MessagePackSerializer.Serialize((BlockInfoPacket)value);
    }
}

