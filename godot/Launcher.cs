// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Godot;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog.Extensions.Logging;
using Utopia.Core;
using Utopia.Core.IO;
using Utopia.Core.Plugin;
using Utopia.Core.Translation;
using static Utopia.Godot.Launcher;
using IContainer = Autofac.IContainer;

namespace Utopia.Godot;

/// <summary>
/// 启动器
/// </summary>
public class Launcher(LaunchOptions options) : Launcher<LaunchOptions>(options)
{

    /// <summary>
    /// 启动参数
    /// </summary>
    public class LaunchOptions(Node node)
    {
        public Node Root { get; set; } = node;

        public ResourceLocator? ResourceLocator { get; set; }
    }

    protected override void _BuildDefaultContainer()
    {
        Builder!
            .RegisterInstance(Option.Root)
            .SingleInstance()
            .As<Node>();
        Builder!
            .RegisterType<TimeProvider>()
            .SingleInstance()
            .AsSelf();
        Builder!
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
            .RegisterInstance(new TaskFactory())
            .AsSelf()
            .SingleInstance();
        Builder
            .RegisterType<PluginLoader<IPlugin>>()
            .SingleInstance()
            .As<IPluginLoader<IPlugin>>();
        Builder.
            RegisterType<TranslationManager>()
            .SingleInstance()
            .As<ITranslationManager>();
        // kestrel server
        Builder
            .RegisterType<KestrelServerOptions>()
            .SingleInstance();
        Builder
            .RegisterType<OptionsWrapper<KestrelServerOptions>>()
            .As<IOptions<KestrelServerOptions>>()
            .SingleInstance();
        Builder
            .RegisterType<SocketTransportOptions>()
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
        // wait for exit

    }
}
