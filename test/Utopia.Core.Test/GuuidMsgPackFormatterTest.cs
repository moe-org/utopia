// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessagePack;

namespace Utopia.Core.Test;
public class GuuidMsgPackFormatterTest
{
    [Fact]
    public void TestGuuidMsgPack()
    {
        Guuid id = new("a", "b", "c");

        var bytes = MessagePackSerializer.Serialize(id);

        var guuid = MessagePackSerializer.Deserialize<Guuid>(bytes);

        Assert.Equal(id, guuid);
    }

    [MessagePackObject]
    public class GuuidInClass
    {
        [Key(0)]
        public int Key1 { get; set; } = 1;

        [Key(1)]
        public Guuid Id1 { get; set; } = new("a", "id1");

        [Key(2)]
        public Guuid Id2 { get; set; } = new("a", "id2");

        [Key(3)]
        public int Key2 { get; set; } = 2;
    }

    [Fact]
    public void TestGuuidMsgPackInClass()
    {
        GuuidInClass id = new();

        var bytes = MessagePackSerializer.Serialize(id);

        var got = MessagePackSerializer.Deserialize<GuuidInClass>(bytes);

        Assert.Equal(id.Key1, got.Key1);
        Assert.Equal(id.Id1, got.Id1);
        Assert.Equal(id.Id2, got.Id2);
        Assert.Equal(id.Key2, got.Key2);
    }

}
