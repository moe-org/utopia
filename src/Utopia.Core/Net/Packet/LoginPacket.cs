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
public partial class LoginPacket
{
    public string PlayerId { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}

public class LoginPacketFormatter : IPacketFormatter
{
    public static readonly Guuid PacketTypeId = Guuid.GetInternalGuuid("Net", "Packet", "Login");

    public object GetValue(Guuid _, ReadOnlySequence<byte> packet)
    {
        return MemoryPackSerializer.Deserialize<LoginPacket>(packet)!;
    }

    public Memory<byte> ToPacket(Guuid _, object value)
    {
        Guard.IsNotNull(value);
        Guard.IsAssignableToType(value, typeof(LoginPacket));
        return MemoryPackSerializer.Serialize((LoginPacket)value);
    }
}