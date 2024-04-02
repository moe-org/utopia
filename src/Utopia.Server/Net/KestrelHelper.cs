// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Utopia.Core.Net;
using Utopia.Core.Net.Middlewares;

namespace Utopia.Server.Net;
public static class KestrelHelper
{
    public static Launcher UseKestrelForServer(this Launcher launcher)
    {
        var builder = launcher.Builder!;
        builder.RegisterType<PacketReadMiddleware>().InstancePerDependency();
        builder.RegisterType<PacketDecodeMiddleware>().InstancePerDependency();
        builder.RegisterType<PacketDispatchMiddleware>().InstancePerDependency();
        builder.RegisterType<PacketEncodeMiddleware>().InstancePerDependency();
        builder.RegisterType<PacketWriteMiddleware>().InstancePerDependency();

        builder.Register<KestrelServerOptions>((context) =>
        {
            var opt = new KestrelServerOptions();

            opt.ListenAnyIP(Launcher.Options.DefaultPort, (endPointOption) =>
            {
                endPointOption
                    .EnableMiddleware(context)
                    .UseMiddleware(context.Resolve<PacketReadMiddleware>())
                    .UseMiddleware(context.Resolve<PacketDecodeMiddleware>())
                    .UseMiddleware(context.Resolve<PacketDispatchMiddleware>())
                    .UseMiddleware(context.Resolve<PacketEncodeMiddleware>())
                    .UseMiddleware(context.Resolve<PacketWriteMiddleware>());
            });
            return opt;
        }).SingleInstance();

        return launcher;
    }
}
