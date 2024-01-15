// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

namespace Utopia.Core.Exceptions;

public class GuuidFormatException : FormatException
{
    public string Guuid { get; init; }

    public string? Reason { get; init; }

    public GuuidFormatException() : this("unknown")
    {

    }

    public GuuidFormatException(string guuid) : this(guuid,null,null)
    {
    }

    public GuuidFormatException(string guuid,Exception? exception) : this(guuid, null, exception)
    {
    }

    public GuuidFormatException(string guuid, string? reason,Exception? exception) : base($"the format of guuid `{guuid}` was broken for {reason}", exception)
    {
        ArgumentNullException.ThrowIfNull(guuid);
        Guuid = guuid;
        Reason = reason;
    }

    public static GuuidFormatException Throw(string guuid,string reason)
    {
        return new GuuidFormatException(guuid, reason,null);
    }
}
