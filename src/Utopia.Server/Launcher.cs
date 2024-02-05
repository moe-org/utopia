// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using Autofac;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Npgsql;
using Utopia.Core;
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
public class Launcher
{
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
        /// 是否跳过初始化log系统
        /// </summary>
        public bool SkipInitLog { get; set; } = false;

        /// <summary>
        /// If it's null,skip set up logging system
        /// </summary>
        public LogManager.LogOption? LogOption { get; set; } = LogManager.LogOption.CreateDefault();

        /// <summary>
        /// 文件系统
        /// </summary>
        public IResourceLocator? FileSystem { get; set; } = null;

        /// <summary>
        /// 数据库链接，must not be null
        /// </summary>
        public NpgsqlDataSource DatabaseSource { get; set; } = null!;

        /// <summary>
        /// What language we want to use.
        /// </summary>
        public LanguageID GlobalLanguage { get; set; } =
            new(CultureInfo.CurrentCulture.TwoLetterISOLanguageName,
            RegionInfo.CurrentRegion.TwoLetterISORegionName);
    }

    /// <summary>
    /// 使用字符串参数启动服务器
    /// </summary>
    /// <param name="args">命令行参数</param>
    public static void LaunchWithArguments(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args, nameof(args));

        var option = new LauncherOption();

        int i = 0;
        while (i != args.Length)
        {
            string arg = args[i++];

            if (arg == "--port")
            {
                if (i == args.LongLength)
                {
                    throw new ArgumentException("--port argument need one number");
                }
                option.Port = int.Parse(args[i++]);
            }
            else if (arg == "--skip-log-init")
            {
                option.SkipInitLog = true;
            }
            else if (arg == "--batch")
            {
                option.LogOption = LogManager.LogOption.CreateBatch();
            }
            else if (arg == "--postgreSql")
            {
                if (i == args.LongLength)
                {
                    throw new ArgumentException("--port argument need one number");
                }
                var dataSourceBuilder = new NpgsqlDataSourceBuilder(args[i++]);
                _ = dataSourceBuilder.UseLoggerFactory(new
                    NLog.Extensions.Logging.NLogLoggerFactory());
                NpgsqlDataSource dataSource = dataSourceBuilder.Build();
                option.DatabaseSource = dataSource;
            }
            else
            {
                throw new ArgumentException("unknown command line argument:" + arg);
            }
        }

        Launch(option);
    }

    private static IContainer _CreateContainer(LauncherOption option)
    {
        Guard.IsNotNull(option);

        ContainerBuilder builder = new();
        builder
            .RegisterInstance(option)
            .SingleInstance()
            .As<LauncherOption>();
        builder
            .RegisterInstance(option.FileSystem ?? new ResourceLocator())
            .SingleInstance()
            .As<IResourceLocator>();
        builder
            .RegisterType<NLogLoggerFactory>()
            .SingleInstance()
            .As<ILoggerFactory>();
        builder
            .RegisterGeneric(typeof(Logger<>))
            .As(typeof(ILogger<>))
            .SingleInstance();
        builder
            .RegisterType<PluginLoader<IPlugin>>()
            .SingleInstance()
            .As<IPluginLoader<IPlugin>>();
        builder.
            RegisterType<TranslationManager>()
            .SingleInstance()
            .As<ITranslationManager>();
        builder
            .RegisterType<StandardLogicThread>()
            .SingleInstance()
            .As<ILogicThread>();
        builder
            .RegisterType<EventBus>()
            .SingleInstance()
            .As<IEventBus>();
        builder
            .RegisterType<EntityManager>()
            .SingleInstance()
            .As<IEntityManager>();
        builder
            .RegisterType<InternetMain>()
            .SingleInstance()
            .As<IInternetMain>();
        builder
            .RegisterType<InternetListener>()
            .SingleInstance()
            .As<IInternetListener>();
        builder
            .RegisterType<ConcurrentDictionary<long, IWorld>>()
            .SingleInstance()
            .As<ConcurrentDictionary<long, IWorld>>();
        builder
            .RegisterType<ConcurrentDictionary<Guuid, IWorldFactory>>()
            .SingleInstance()
            .As<ConcurrentDictionary<Guuid, IWorldFactory>>();
        builder
            .RegisterType<MainThread>()
            .AsSelf()
            .SingleInstance();
        builder
            .RegisterType<StandardPluginProvider>()
            .As<IPluginProvider>()
            .SingleInstance();

        return builder.Build();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="option"></param>
    /// <param name="startTokenSource">when the server started,cancel the token</param>
    /// <returns></returns>
    public static void Launch(LauncherOption option,CancellationTokenSource? startTokenSource = null)
    {
        ArgumentNullException.ThrowIfNull(option);
        if(option.LogOption != null)
        {
            LogManager.Init(option.LogOption);
        }

        var container = _CreateContainer(option);

        var headquarters = container.Resolve<MainThread>();

        startTokenSource ??= new();

        // set an timer
        TimeUtilities.SetAnNoticeWhenCancel(
            container.Resolve<ILogger<Launcher>>(), "Server", startTokenSource.Token);

        headquarters.Launch(startTokenSource);
    }

    private static void Main(string[] args) => LaunchWithArguments(args);
}
