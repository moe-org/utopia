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
using Xunit.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Utopia.Core.Logging;

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

    private readonly object _lock = new();

    private readonly List<(string logger, string message)> logs = [];

    public IEnumerable<(string logger, string message)> Logs
    {
        get
        {
            lock (_lock)
            {
                return logs;
            }
        }
    }

    public TestableLauncher(Options options, ITestOutputHelper output) : base(options)
    {
        Builder!.RegisterInstance<IFileSystem>(FileSystem).SingleInstance();
        Builder!
            .Register((_) =>
        {
            ServiceCollection services = new();
            services.AddLogging((logging) =>
                {
                    logging.AddXUnit(output);
                    logging.AddProvider(new LogManager.WarppedLoggerProvider((name, msg) =>
                    {
                        lock (_lock)
                        {
                            logs.Add((name, msg));
                        }
                    }));
                }
            );
            return services.BuildServiceProvider().GetRequiredService<ILoggerFactory>();
        })
            .As<ILoggerFactory>()
            .SingleInstance();
    }
}
