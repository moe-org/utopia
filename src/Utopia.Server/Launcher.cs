// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using Autofac;
using AutoMapper;
using CommandLine;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using NLog.Fluent;
using Npgsql;
using Utopia.Core;
using Utopia.Core.Exceptions;
using Utopia.Core.IO;
using Utopia.Core.Logging;
using Utopia.Core.Plugin;
using Utopia.Core.Translation;
using Utopia.Server.Entity;
using Utopia.Server.Logic;
using Utopia.Server.Map;
using Utopia.Server.Net;

namespace Utopia.Server;

/// <summary>
/// 服务器启动器
/// </summary>
public class Launcher : IDisposable
{
    /// <summary>
    /// 仅在内部使用。用于从命令行参数解析。
    /// </summary>
    private enum LogOption { Default, Batch }

    private class CommandLineOption
    {
        [Option] public int Port { get; set; } = 1145;

        [Option] public LogOption LogOption { get; set; } = LogOption.Default;

        [Option] public string ServerRoot { get; set; } = ".";

        [Option] public string? PostgreSQLConnection { get; set; } = null;

        [Option] public string Culture { get; set; } = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

        [Option] public string Region { get; set; } = RegionInfo.CurrentRegion.TwoLetterISORegionName;
    }

    /// <summary>
    /// 启动参数
    /// </summary>
    public class LauncherOption
    {
        /// <summary>
        /// 服务器端口
        /// </summary>
        public int Port { get; set; } = 1145;

        /// <summary>
        /// If it's null,skip set up logging system
        /// </summary>
        public LogManager.LogOption? LogOption { get; set; } = LogManager.LogOption.CreateDefault();

        /// <summary>
        /// 文件系统
        /// </summary>
        public IResourceLocator? ResourceLocator { get; set; } = null;

        /// <summary>
        /// 数据库链接
        /// </summary>
        public NpgsqlDataSource? DatabaseSource { get; set; } = null;

        /// <summary>
        /// What language we want to use.
        /// </summary>
        public LanguageID GlobalLanguage { get; set; } =
            new(CultureInfo.CurrentCulture.TwoLetterISOLanguageName,
            RegionInfo.CurrentRegion.TwoLetterISORegionName);

        /// <summary>
        /// 使用字符串参数解析
        /// </summary>
        /// <param name="args">命令行参数</param>
        public static LauncherOption ParseOptions(string[] args)
        {
            ArgumentNullException.ThrowIfNull(args, nameof(args));
            LauncherOption option = new();

            Parser.Default
                        .ParseArguments<CommandLineOption>(args)
                        .WithParsed((opt) =>
                        {
                            // TODO: REPLACE WITH AUTOMAPPER
                            option.Port = opt.Port;
                            option.ResourceLocator = new ResourceLocator(opt.ServerRoot);

                            if (option.DatabaseSource is not null)
                            {
                                var dataSourceBuilder = new NpgsqlDataSourceBuilder(opt.PostgreSQLConnection);
                                dataSourceBuilder.UseLoggerFactory(new NLogLoggerFactory());
                                NpgsqlDataSource dataSource = dataSourceBuilder.Build();

                                option.DatabaseSource = dataSource;
                            }

                            option.GlobalLanguage = new LanguageID(opt.Culture, opt.Region);

                            option.LogOption = opt.LogOption switch
                            {
                                Launcher.LogOption.Batch => LogManager.LogOption.CreateBatch(),
                                Launcher.LogOption.Default => LogManager.LogOption.CreateDefault(),
                                _ => option.LogOption
                            };
                        });

            return option;
        }
    }

    private WeakThreadSafeEventSource<IContainer> _source = new();

    public event Action<IContainer> GameLaunch
    {
        add
        {
            _source.Register(value);
        }
        remove
        {
            _source.Unregister(value);
        }
    }

    public ContainerBuilder Builder { get; set; } = new();

    public bool Launched { get; private set; } = false;

    public LauncherOption Option { get; }

    public Launcher(LauncherOption option)
    {
        Guard.IsNotNull(option);

        Option = option;
        // register default components.
        // this is must for a basic game.
        _RegisterDefault();
    }

    private void _RegisterDefault()
    {
        Builder
            .RegisterInstance(Option)
            .SingleInstance()
            .As<LauncherOption>();
        Builder
            .RegisterInstance(Option.ResourceLocator ?? new ResourceLocator("."))
            .SingleInstance()
            .As<IResourceLocator>();
        Builder
            .RegisterType<NLogLoggerFactory>()
            .SingleInstance()
            .As<ILoggerFactory>();
        Builder
            .RegisterGeneric(typeof(Logger<>))
            .As(typeof(ILogger<>))
            .SingleInstance();
        Builder
            .RegisterType<PluginLoader<IPlugin>>()
            .SingleInstance()
            .As<IPluginLoader<IPlugin>>();
        Builder.
            RegisterType<TranslationManager>()
            .SingleInstance()
            .As<ITranslationManager>();
        Builder
            .RegisterType<StandardLogicThread>()
            .SingleInstance()
            .As<ILogicThread>();
        Builder
            .RegisterType<EventBus>()
            .SingleInstance()
            .As<IEventBus>();
        Builder
            .RegisterType<EntityManager>()
            .SingleInstance()
            .As<IEntityManager>();
        Builder
            .RegisterType<InternetMain>()
            .SingleInstance()
            .As<IInternetMain>();
        Builder
            .RegisterType<InternetListener>()
            .SingleInstance()
            .As<IInternetListener>();
        Builder
            .RegisterType<ConcurrentDictionary<Guuid, IWorld>>()
            .SingleInstance()
            .AsSelf();
        Builder
            .RegisterType<ConcurrentDictionary<Guuid, IWorldFactory>>()
            .SingleInstance()
            .AsSelf();
        Builder
            .RegisterType<MainThread>()
            .AsSelf()
            .SingleInstance();
        Builder
            .RegisterType<StandardPluginProvider>()
            .As<IPluginProvider>()
            .SingleInstance();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~Launcher()
    {
        Dispose(false);
    }

    private bool _diposed = false;

    private void Dispose(bool disposing)
    {
        if (_diposed)
        {
            return;
        }

        if (disposing)
        {

        }

        _diposed = true;
    }

    public IContainer? Container { get; private set; } = null;

    public Task? MainThreadTask { get; private set; } = null;

    public void Launch()
    {
        // check flags
        if (Launched)
        {
            throw new InvalidOperationException("you can not launch the game more than once");
        }
        if (_diposed)
        {
            throw new InvalidOperationException("the launcher was disposed");
        }

        Launched = true;

        // build container
        Container = Builder.Build();

        // fire event
        _source.Fire(Container);

        // set an notice to users
        var mainThread = Container.Resolve<MainThread>();

        TimeUtilities.SetAnNoticeWhenComplete(
            Container.Resolve<ILogger<Launcher>>(), "Server", mainThread.StartTask);

        // start the main thread
        TaskCompletionSource source = new();
        var t = new Thread(() =>
        {
            try
            {
                mainThread.Launch();
            }
            catch (Exception e)
            {
                source.SetException(e);
            }
            finally
            {
                source.TrySetResult();
            }
        });

        t.Start();

        MainThreadTask = source.Task;
    }
}
