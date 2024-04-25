// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Utopia.Server.Launcher;
using Utopia.Core;
using Autofac;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

namespace Utopia.Server.Test;

public class TestableLauncher : Launcher
{
    public static Options GetDefaultTestOption()
    {
        var opt = Options.Default();
        opt.LogOption = new Core.Logging.LogManager.LogOption()
        {
            EnableLoggingSystem = false,
        };
        return opt;
    }

    public readonly MockFileSystem FileSystem = new();

    public TestableLauncher() : base(GetDefaultTestOption())
    {
        // TODO: Register Tastable logger
        // TODO: Register Testable things
        Builder!.RegisterInstance<IFileSystem>(FileSystem).SingleInstance();
        // Builder!.RegisterInstance<>
    }
}
