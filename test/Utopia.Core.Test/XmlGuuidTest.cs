// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Utopia.Core.Test;
public class XmlGuuidTest
{

    public class GuuidInXml
    {
        [XmlElement]
        public XmlGuuid id = new(new("root", "test"));
    }

    [Fact]
    public void XmlGuuidSerializeAndDeserializeTest()
    {
        XmlSerializer serializer = new(typeof(GuuidInXml));
        var clazz = new GuuidInXml();

        MemoryStream stream = new();
        serializer.Serialize(stream, clazz);

        stream = new(stream.ToArray());

        var got = serializer.Deserialize<GuuidInXml>(stream);

        Assert.Equal(clazz.id.Guuid, got.id.Guuid);
    }
}
