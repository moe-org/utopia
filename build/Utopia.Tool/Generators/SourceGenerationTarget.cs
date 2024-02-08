// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System.Text;

namespace Utopia.Tool.Generators;

public class SourceGenerationTarget
{
    public required string Text { get; init; }

    public required string ShouldWriteTo { get; init; }

    public void Write(string? writeTo)
    {
        if (writeTo is null)
        {
            writeTo = ShouldWriteTo;
        }

        File.WriteAllText(writeTo,Text,Encoding.UTF8);
    }
}
