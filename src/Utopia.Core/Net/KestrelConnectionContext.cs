// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Utopia.Core.Mnagement;

namespace Utopia.Core.Net;

/// <summary>
/// 代表单个Kestrel连接上下文。
/// </summary>
public class KestrelConnectionContext
{
    public required IFeatureCollection Features { get; init; }

    public required User? user { get; set; }

    public required IDispatcher Dispatcher { get; init; }

    public required IPacketizer Packetizer { get; init; }

    public required ILifetimeScope LifetimeScope { get; init; }

    public required ConnectionContext Connection { get; init; }
}
