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
using Microsoft.Extensions.Logging;
using Utopia.Core;
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

    }

    protected override void _BuildDefaultContainer()
    {
        Builder!
            .RegisterInstance(Option.Root)
            .SingleInstance()
            .As<Node>();
    }

    protected override void _MainThread()
    {
        // wait for exit

    }
}
