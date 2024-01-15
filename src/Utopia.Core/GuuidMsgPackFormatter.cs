// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System.Text;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using MessagePack;

namespace Utopia.Core;

public class GuuidMsgPackFormatter : IMessagePackFormatter<Guuid>
{
    public static readonly GuuidMsgPackFormatter Instance = new();

    public Guuid Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        options.Security.DepthStep(ref reader);

        string? id = reader.ReadString();

        reader.Depth--;

        return Guuid.Parse(id!)!;
    }

    public void Serialize(ref MessagePackWriter writer, Guuid value, MessagePackSerializerOptions options)
    {

        string v = value.ToString();

        writer.WriteString(Encoding.UTF8.GetBytes(v));
    }

    public static MessagePackSerializerOptions CreateOption()
    {
        IFormatterResolver resolver = CompositeResolver.Create(
            new[] { Instance },
             new[] { StandardResolver.Instance });

        return MessagePackSerializerOptions.Standard.WithResolver(resolver);
    }
}

