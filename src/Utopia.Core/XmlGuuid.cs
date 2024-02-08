#region

using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

#endregion

namespace Utopia.Core;

/// <summary>
///     这个类用于GUUID的XML的序列化。
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
        this.Guuid = guuid;
    }

    public XmlSchema? GetSchema()
    {
        return null;
    }

    public void ReadXml(XmlReader reader)
    {
        this.Guuid = Guuid.Parse(reader.ReadElementContentAsString());
    }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteString(this.Guuid.ToString());
    }

    public override bool Equals(object? obj)
    {
        return this.Guuid.Equals(obj);
    }

    public override int GetHashCode()
    {
        return this.Guuid.GetHashCode();
    }

    public override string ToString()
    {
        return this.Guuid.ToString();
    }

    public static XmlSchemaSimpleType GetGuuidSchemaOfType(XmlSchemaSet set)
    {
        XmlSchemaSimpleType type = new();

        {
            XmlSchemaSimpleTypeRestriction restriction = new();
            restriction.BaseTypeName = new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema");
            {
                XmlSchemaPatternFacet facet = new();
                facet.Value = Guuid.Pattern;
                restriction.Facets.Add(facet);
            }
            type.Content = restriction;
        }

        return type;
    }

    /// <summary>
    /// 隐式转换到<see cref="Guuid"/>
    /// </summary>
    /// <param name="guuid"></param>
    /// <returns></returns>
    public static explicit operator Guuid(XmlGuuid guuid)
    {
        return guuid.Guuid;
    }
}
