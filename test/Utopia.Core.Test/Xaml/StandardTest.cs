// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utopia.Core.Configuration.Xaml;
using Utopia.Core.Configuration.Xaml.Standard;
using Utopia.Shared;
using XamlX.Parsers;

[assembly: XmlnsDefinition(XmlNamespace.Utopia + "/test", "Utopia.Core.Test.Xaml")]
namespace Utopia.Core.Test.Xaml;

public class SimpleSubClass
{
    public string Test { get; set; } = string.Empty;
}

public class TestXamlClass
{
    public string Test { get; set; } = string.Empty;

    public string Test2 { get; set; } = string.Empty;

    [Content]
    public IEnumerable<SimpleSubClass> Children { get; set; } = new List<SimpleSubClass>();
}

/// <summary>
/// test our Xaml language standard and make it a document.
/// </summary>
public class StandardTest
{

    public static Compiler GetCompiler()
    {
        var config = StandardConfiguration.CreateDefault();

        return Compiler.CreateFrom(config.Item1, config.Item2);
    }

    [Fact]
    public void ParseTest()
    {
        var compiler = GetCompiler();

        var source =
            $"""
            <TestXamlClass xmlns="{XmlNamespace.Utopia}/test" Test="test 1">
                <SimpleSubClass Test="sub test 1" />
                <TestXamlClass.Test2>test 2</TestXamlClass.Test2>
                <SimpleSubClass Test="sub test 2" />
                <SimpleSubClass Test="sub test 3" />
            </TestXamlClass>
            """;

        var obj = compiler?.Compile(source).create!(null)!;

        Assert.True(obj is TestXamlClass);
        TestXamlClass test = (TestXamlClass)obj;
        Assert.Equal("test 1", test.Test);
        Assert.Equal("test 2", test.Test2);

        Assert.Equal(3, test.Children.Count());
        Assert.Equal("sub test 1", test.Children.ToList()[0].Test);
        Assert.Equal("sub test 2", test.Children.ToList()[1].Test);
        Assert.Equal("sub test 3", test.Children.ToList()[2].Test);
    }

}
