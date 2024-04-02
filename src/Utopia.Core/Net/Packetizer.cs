#region

using System.Buffers;
using System.Collections.Concurrent;

#endregion

namespace Utopia.Core.Net;

/// <summary>
///     包序列化/逆序列化器.
///     要求是线程安全的.
/// </summary>
public interface IPacketizer
{
    public ConcurrentDictionary<Guuid, IPacketFormatter> Formatters { get; }

    /// <summary>
    ///     把字节序列转换为包.
    /// </summary>
    public object Decode(Guuid packetTypeId, ReadOnlySequence<byte> data);

    /// <summary>
    ///     把包转化为字节序列
    /// </summary>
    public Memory<byte> Encode(Guuid packetTypeId, object obj);
}

/// <summary>
///     分包器,是线程安全的.
/// </summary>
public class Packetizer : IPacketizer
{
    public ConcurrentDictionary<Guuid, IPacketFormatter> Formatters { get; init; } = new();

    public object Decode(Guuid packetTypeId, ReadOnlySequence<byte> data)
    {
        if (!this.Formatters.TryGetValue(packetTypeId, out var formatter))
            throw new InvalidOperationException("unknown packet type id");

        return formatter.GetValue(packetTypeId, data);
    }

    public Memory<byte> Encode(Guuid packetTypeId, object obj)
    {
        if (!this.Formatters.TryGetValue(packetTypeId, out var formatter))
            throw new InvalidOperationException("unknown packet type id");

        return formatter.ToPacket(packetTypeId, obj);
    }
}
