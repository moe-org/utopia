// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utopia.Core.Configuration;
using Zio.FileSystems;

namespace Utopia.Core.Test;

/// <summary>
/// test for configuation compiler
/// </summary>
public class Configuration
{
    public class Option
    {
        public int Value { get; set; } = 0;

        public bool Accessed { get; set; } = false;

        public Encoding Encoding { get; set; } = Encoding.UTF8;
    }

    [Fact]
    public async Task ConfigurationRunTest()
    {
        ConfigurationCompiler compiler = new(new PhysicalFileSystem());

        Option opt = new();

        Assert.Equal(0, opt.Value);
        Assert.False(opt.Accessed);

        await compiler.InvokeFor(
            """
            config.Value = 1;
            config.Accessed = true;
            """,
            opt);

        Assert.Equal(1, opt.Value);
        Assert.True(opt.Accessed);
    }


    [Fact]
    public async Task ConfigurationImportAndRunTest()
    {
        ConfigurationCompiler compiler = new(new PhysicalFileSystem());

        Option opt = new();

        Assert.Equal(Encoding.UTF8, opt.Encoding);

        await compiler.InvokeFor(
            """
            using System.Text;
            config.Encoding = Encoding.Unicode;
            """,
            opt);

        Assert.Equal(Encoding.Unicode, opt.Encoding);
    }

}
