// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using Autofac;
using AutoMapper;
using CommandLine;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using NLog.Fluent;
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
    private enum LogOption{ Default,Batch }

    private Launcher()
    {
    }

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
        public IResourceLocator? FileSystem { get; set; } = null;

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
    }

    /// <summary>
    /// 使用字符串参数启动服务器
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
                            option.FileSystem = new ResourceLocator(opt.ServerRoot);

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
                                LogOption.Batch   => LogManager.LogOption.CreateBatch(),
                                LogOption.Default => LogManager.LogOption.CreateDefault(),
                                _                 => option.LogOption
                            };
                        });

        return option;
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
            .RegisterInstance(option.FileSystem ?? new ResourceLocator("."))
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
            .RegisterType<ConcurrentDictionary<Guuid, IWorld>>()
            .SingleInstance()
            .AsSelf();
        builder
            .RegisterType<ConcurrentDictionary<Guuid, IWorldFactory>>()
            .SingleInstance()
            .AsSelf();
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
    /// 使用参数启动服务器。
    /// </summary>
    /// <param name="option">参数</param>
    /// <param name="startTask">启动task。将会在启动后complete.</param>
    /// <returns>服务器主线程Task.会根据服务器运行状态设置<see cref="Task.IsCompleted"/>,<see cref="Task.Exception"/>等内容。</returns>
    public static Task Launch(LauncherOption option,out Task startTask)
    {
        // prepare
        ArgumentNullException.ThrowIfNull(option);
        if(option.LogOption != null)
        {
            LogManager.Init(option.LogOption);
        }

        var container = _CreateContainer(option);

        var mainThread = container.Resolve<MainThread>();

        // set an timer
        startTask = mainThread.StartTask;
        TimeUtilities.SetAnNoticeWhenCancel(
            container.Resolve<ILogger<Launcher>>(), "Server", mainThread.StartTask);

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

        return source.Task;
    }

}
