// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using MessagePack;
using Utopia.Core;

namespace Utopia.Core.Test;

public class GuuidTest
{

    [Fact]
    public void TestGuuidToStringAndParseStringWorksWell()
    {
        var guuid = new Guuid("root", "node");
        string str = guuid.ToString();

        var parsed = Guuid.Parse(str);

        Assert.Equal(guuid, parsed);
        Assert.Equal(guuid.GetHashCode(), parsed.GetHashCode());
    }

    [Fact]
    public void GuuidEqualTest()
    {
        Guuid one = new("root", "one");
        Guuid oneToo = new("root", "one");
        Guuid two = new("root", "two");

        Assert.Equal(one, oneToo);
        Assert.NotEqual(one, two);
        Assert.NotEqual(oneToo, two);
    }

    [Fact]

    public void CheckIllegalNames()
    {
        bool parsed = Guuid.CheckName(string.Empty);

        Assert.False(parsed);
    }

    [Theory]
    [InlineData("", "nonempty")]
    [InlineData(null, "nonempty")]
    [InlineData("nonempty", "")]
    [InlineData("nonempty", null)]
    public void TestGuuidParseStringParseIllegal(string root, string node) => Assert.Throws<ArgumentException>(() =>
                                                                                   {
                                                                                       _ = new Guuid(root, node);
                                                                                   });

    [Theory]
    [InlineData("a", new string[] {"b"})]
    [InlineData("a", new string[] { "b", "c" })]
    [InlineData("a", new string[] { "b", "c", "d" })]
    public void TestGuuidChildCheckFailureMethod(string root,params string[] fatherNodes)
    {
        var father = new Guuid(root,fatherNodes);
        var child = new Guuid("a", "b", "c", "d");

        var success = father.HasChild(child);

        Assert.True(success);
    }

    [Theory]
    [InlineData("a", new string[] { "b" })]
    [InlineData("a", new string[] { "b", "c" })]
    public void TestGuuidChildCheckMethod(string root, params string[] fatherNodes)
    {
        var father = new Guuid(root, fatherNodes);
        var child = new Guuid("a", "b", "c", "d");

        var failure = child.HasChild(father);

        Assert.False(failure);
    }

    [Fact]
    public void TestGuuidGetParent()
    {
        var guuid = new Guuid("a", "b", "c", "d");

        Assert.Equal(new Guuid("a", "b", "c"), guuid.GetParent());
        Assert.Equal(new Guuid("a", "b"), guuid.GetParent()!.Value.GetParent());
        Assert.Null(guuid.GetParent()!.Value.GetParent()!.Value.GetParent());
    }
}
