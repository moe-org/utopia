// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml;

namespace Utopia.Core;

/// <summary>
/// 这个类用于GUUID的XML的序列化。
/// </summary>
[XmlSchemaProvider(nameof(GetGuuidSchemaOfType))]
public sealed class XmlGuuid : IXmlSerializable
{
    public Guuid Guuid { get; set; } = Guuid.Empty;

    public XmlGuuid()
    {

    }

    public XmlGuuid(Guuid guuid)
    {
        Guuid = guuid;
    }

    public override bool Equals(object? obj) => Guuid.Equals(obj);

    public override int GetHashCode() => Guuid.GetHashCode();

    public override string ToString() => Guuid.ToString();

    public static XmlSchemaSimpleType GetGuuidSchemaOfType(XmlSchemaSet set)
    {
        XmlSchemaSimpleType type = new();

        {
            XmlSchemaSimpleTypeRestriction restriction = new();
            restriction.BaseTypeName = new("string", "http://www.w3.org/2001/XMLSchema");
            {
                XmlSchemaPatternFacet facet = new();
                facet.Value = Guuid.Pattern;
                restriction.Facets.Add(facet);
            }
            type.Content = restriction;
        }

        return type;
    }

    public XmlSchema? GetSchema()
    {
        return null;
    }

    public void ReadXml(XmlReader reader)
    {
        Guuid = Guuid.Parse(reader.ReadElementContentAsString());
    }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteString(Guuid.ToString());
    }
}
