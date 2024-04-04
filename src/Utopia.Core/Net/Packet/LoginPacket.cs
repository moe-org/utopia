#region

using System.Buffers;
using CommunityToolkit.Diagnostics;
using MemoryPack;

#endregion

namespace Utopia.Core.Net.Packet;

/// <summary>
///     登录包
/// </summary>
[MemoryPackable]
public partial class LoginPacket : IWithPacketId
{
    public static Guuid PacketID => InternalHelper.NewInternalGuuid("Net", "Packet", "Login");

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}

