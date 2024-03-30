namespace Utopia.Core.Exceptions;

public class GuuidFormatException : FormatException
{
    public GuuidFormatException() : this("unknown")
    {
    }

    public GuuidFormatException(string guuid) : this(guuid, null, null)
    {
    }

    public GuuidFormatException(string guuid, Exception? exception) : this(guuid, null, exception)
    {
    }

    public GuuidFormatException(string guuid, string? reason, Exception? exception) : base(
        $"the format of guuid `{guuid}` was broken for {reason}", exception)
    {
        ArgumentNullException.ThrowIfNull(guuid);
        this.Guuid = guuid;
        this.Reason = reason;
    }

    public string Guuid { get; init; }

    public string? Reason { get; init; }

    public static GuuidFormatException Throw(string guuid, string reason)
    {
        return new GuuidFormatException(guuid, reason, null);
    }
}
