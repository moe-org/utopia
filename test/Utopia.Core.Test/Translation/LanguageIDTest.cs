#region

using Utopia.Core.Translation;

#endregion

namespace Utopia.Core.Test.Translation;

public class LanguageIDTest
{
    [Fact]
    public void ConstructTest()
    {
        _ = new LanguageID("aa", "aa");
    }

    [Fact]
    public void BadConstructTest()
    {
        Assert.Throws<FormatException>(() => new LanguageID(string.Empty, "aa"));
        Assert.Throws<ArgumentNullException>(() => new LanguageID(null!, "aa"));
        Assert.Throws<FormatException>(() => new LanguageID("a", "aa"));
        Assert.Throws<FormatException>(() => new LanguageID("a", "aaa"));
        Assert.Throws<FormatException>(() => new LanguageID("aaa", "aaa"));
        Assert.Throws<FormatException>(() => new LanguageID("aa", "aaa"));
    }

    [Fact]
    public void ParseTest()
    {
        LanguageID.Parse("aa-aa");
        LanguageID.Parse("aa_aa");
        LanguageID.Parse("aa aa");
    }

    [Fact]
    public void BadParseTest()
    {
        Assert.Throws<FormatException>(() => LanguageID.Parse("aaa-aa"));
        Assert.Throws<FormatException>(() => LanguageID.Parse("aa_a"));
        Assert.Throws<FormatException>(() => LanguageID.Parse("aa+aa"));
        Assert.Throws<FormatException>(() => LanguageID.Parse("aa=aa"));
        Assert.Throws<FormatException>(() => LanguageID.Parse("aa  aa"));
    }

    [Fact]
    public void EqualTest()
    {
        LanguageID one = new("zh", "cn");
        LanguageID two = new("Zh", "cN");

        Assert.Equal(one.Language, two.Language);
        Assert.Equal(one.Location, two.Location);
        Assert.Equal(one, two);
        Assert.True(one == two);
        Assert.False(one != two);
        Assert.Equal(one.GetHashCode(), two.GetHashCode());
        Assert.Equal(one.ToString(), two.ToString());
    }

    [Fact]
    public void NotEqualTest()
    {
        LanguageID one = new("zH", "Cn");
        LanguageID two = new("EN", "us");

        Assert.NotEqual(one.Language, two.Language);
        Assert.NotEqual(one.Location, two.Location);
        Assert.NotEqual(one, two);
        Assert.False(one == two);
        Assert.True(one != two);
        Assert.NotEqual(one.GetHashCode(), two.GetHashCode());
        Assert.NotEqual(one.ToString(), two.ToString());
    }
}
