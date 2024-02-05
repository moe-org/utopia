#region

using System.Xml.Serialization;

#endregion

namespace Utopia.Core.Test;

public class XmlGuuidTest
{
    [Fact]
    public void XmlGuuidSerializeAndDeserializeTest()
    {
        XmlSerializer serializer = new(typeof(GuuidInXml));
        var clazz = new GuuidInXml();

        MemoryStream stream = new();
        serializer.Serialize(stream, clazz);

        stream = new MemoryStream(stream.ToArray());

        var got = serializer.Deserialize<GuuidInXml>(stream);

        Assert.Equal(clazz.id.Guuid, got.id.Guuid);
    }

    public class GuuidInXml
    {
        [XmlElement] public XmlGuuid id = new(new Guuid("root", "test"));
    }
}