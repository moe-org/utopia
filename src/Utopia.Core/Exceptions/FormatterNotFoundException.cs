namespace Utopia.Core.Exceptions;

/// <summary>
///     The formatters(see <see cref="Net.IPacketFormatter" />) not found
/// </summary>
internal class PacketFormatterNotFoundExceptiom : Exception
{
    public readonly Guuid PacketId;

    public PacketFormatterNotFoundExceptiom(Guuid packetId) : base(packetId.ToString())
    {
        this.PacketId = packetId;
    }
}