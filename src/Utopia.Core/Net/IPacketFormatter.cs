#region

using System.Buffers;

#endregion

namespace Utopia.Core.Net;

/// <summary>
///     包格式化器
/// </summary>
public interface IPacketFormatter
{
    object GetValue(Guuid packetId, ReadOnlySequence<byte> packet);

    Memory<byte> ToPacket(Guuid packetId, object value);
}