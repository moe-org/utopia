// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.IO.Abstractions;
using System.Net;
using System.Reflection;
using Autofac;
using AutoMapper;
using CommandLine;
using CommunityToolkit.Diagnostics;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog.Extensions.Logging;
using NLog.Fluent;
using Npgsql;
using Utopia.Core;
using Utopia.Core.Exceptions;
using Utopia.Core.IO;
using Utopia.Core.Logging;
using Utopia.Core.Net;
using Utopia.Core.Plugin;
using Utopia.Core.Translation;
using Utopia.Server.Entity;
using Utopia.Server.Logic;
using Utopia.Server.Map;
using Utopia.Server.Net;
using static Utopia.Server.Launcher;

namespace Utopia.Server;

/// <summary>
/// 服务器启动器
/// </summary>
public class Launcher(Launcher.Options option) : Launcher<Launcher.Options>(option)
{
    /// <summary>
    /// 启动参数
    /// </summary>
    public class Options
    {
        /// <summary>
        /// 服务器端口
        /// </summary>
        public const int DefaultPort = 1145;

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

        public KestrelServerOptions KestrelOptions { get; set; } = new();

        public SocketTransportOptions SocketOptions { get; set; } = new();

        private Options()
        {

        }

        public static Options Default()
        {
            return new();
        }
    }
    protected override void _BuildDefaultContainer()
    {
        ArgumentNullException.ThrowIfNull(Builder);
        // some fundation utilities
        Builder
            .RegisterInstance(Option)
            .SingleInstance()
            .As<Options>();
        Builder
            .RegisterType<TimeProvider>()
            .SingleInstance()
            .AsSelf();
        Builder
            .RegisterInstance(Option.ResourceLocator ?? new ResourceLocator(".", new FileSystem()))
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
        /*Builder
            .RegisterType<InternetMain>()
            .SingleInstance()
            .As<IInternetMain>();
        Builder
            .RegisterType<InternetListener>()
            .SingleInstance()
            .As<IInternetListener>();*/
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

        // kestrel server
        Builder
            .RegisterInstance(Option.KestrelOptions)
            .SingleInstance();
        Builder
            .RegisterType<OptionsWrapper<KestrelServerOptions>>()
            .As<IOptions<KestrelServerOptions>>()
            .SingleInstance();
        Builder
            .RegisterInstance(Option.SocketOptions)
            .SingleInstance();
        Builder
            .RegisterType<OptionsWrapper<SocketTransportOptions>>()
            .As<IOptions<SocketTransportOptions>>()
            .SingleInstance();
        Builder
            .RegisterType<SocketTransportFactory>()
            .SingleInstance()
            .As<IConnectionListenerFactory>()
            .As<IConnectionListenerFactorySelector>();
        Builder
            .RegisterType<KestrelServer>()
            .SingleInstance();
    }

    protected override void _MainThread()
    {
        TimerUtilities.SetAnNoticeWhenComplete(
            Container!.Resolve<ILogger<Launcher>>(), "Server", MainTask!);
        var mainThread = Container!.Resolve<MainThread>();
        mainThread.Launch();
    }
}
