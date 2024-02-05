#region

using Autofac;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Utopia.Core.Logging;
using Utopia.Core.Net;
using Xunit.Abstractions;

#endregion

namespace Utopia.Core.Test;

public static class ContainerManager
{
    public static readonly Lazy<IContainer> Container = new(() =>
    {
        LogManager.Init(new LogManager.LogOption
        {
            ColorfulOutput = true,
            EnableConsoleTraceOutput = true,
            EnableConsoleOutput = true,
            EnableLogFile = false
        });

        ContainerBuilder builder = new();

        builder
            .RegisterType<NLogLoggerFactory>()
            .SingleInstance()
            .As<ILoggerFactory>();
        builder
            .RegisterGeneric(typeof(Logger<>))
            .As(typeof(ILogger<>))
            .SingleInstance();
        builder
            .RegisterType<GlobalUDPManager>()
            .SingleInstance();

        return builder.Build();
    }, true);

    public static IContainer GetContainerWithLogger(ITestOutputHelper helper)
    {
        LogManager.Init(new LogManager.LogOption
        {
            ColorfulOutput = true,
            EnableConsoleTraceOutput = true,
            EnableConsoleOutput = true,
            EnableLogFile = false,
            WriteLineAction = helper.WriteLine
        });

        ContainerBuilder builder = new();

        builder
            .RegisterType<NLogLoggerFactory>()
            .SingleInstance()
            .As<ILoggerFactory>();
        builder
            .RegisterGeneric(typeof(Logger<>))
            .As(typeof(ILogger<>))
            .SingleInstance();
        builder
            .RegisterType<GlobalUDPManager>()
            .SingleInstance();

        return builder.Build();
    }
}