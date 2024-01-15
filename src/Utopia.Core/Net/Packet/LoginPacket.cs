// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System.Buffers;
using CommunityToolkit.Diagnostics;
using MessagePack;

namespace Utopia.Core.Net.Packet;

/// <summary>
/// 登录包
/// </summary>
[MessagePackObject]
public class LoginPacket
{
    [Key(0)]
    public string PlayerId { get; set; } = string.Empty;

    [Key(1)]
    public string Password { get; set; } = string.Empty;
}

public class LoginPacketFormatter : IPacketFormatter
{
    public static readonly Guuid PacketTypeId = Guuid.GetInternalGuuid("Net", "Packet", "Login");

    public object GetValue(Guuid _, ReadOnlySequence<byte> packet) => MessagePackSerializer.Deserialize<LoginPacket>(packet);

    public Memory<byte> ToPacket(Guuid _, object value)
    {
        Guard.IsNotNull(value);
        Guard.IsAssignableToType(value, typeof(LoginPacket));
        return MessagePackSerializer.Serialize((LoginPacket)value);
    }
}
