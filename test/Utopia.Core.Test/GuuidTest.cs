using System.Text.RegularExpressions;

namespace Utopia.Core.Test;

public class GuuidTest
{
    private readonly Regex _pattern = new Regex(Guuid.Pattern);
    
    [Fact]
    public void TestGuuidToStringAndParseStringWorksWell()
    {
        var guuid = new Guuid("root", "node");
        var str = guuid.ToString();

        var parsed = Guuid.Parse(str);

        Assert.Equal(guuid, parsed);
        Assert.Equal(guuid.GetHashCode(), parsed.GetHashCode());
        Assert.Matches(this._pattern, guuid.ToString());
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
        var parsed = Guuid.CheckName(string.Empty);

        Assert.False(parsed);
    }

    [Theory]
    [InlineData("", "nonempty")]
    [InlineData(null, "nonempty")]
    [InlineData("nonempty", "")]
    [InlineData("nonempty", null)]
    public void TestGuuidParseStringParseIllegal(string root, string node)
    {
        Assert.Throws<ArgumentException>(() => { _ = new Guuid(root, node); });
    }

    [Theory]
    [InlineData("a", new[] { "b" })]
    [InlineData("a", new[] { "b", "c" })]
    [InlineData("a", new[] { "b", "c", "d" })]
    public void TestGuuidChildCheckFailureMethod(string root, params string[] fatherNodes)
    {
        var father = new Guuid(root, fatherNodes);
        var child = new Guuid("a", "b", "c", "d");

        var success = father.HasChild(child);

        Assert.True(success);
    }

    [Theory]
    [InlineData("a", new[] { "b" })]
    [InlineData("a", new[] { "b", "c" })]
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