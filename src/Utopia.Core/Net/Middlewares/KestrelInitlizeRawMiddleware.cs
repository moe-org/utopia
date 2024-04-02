// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Connections;
using Autofac;

namespace Utopia.Core.Net.Middlewares;

public class KestrelInitlizeRawMiddleware
{
    public required ILifetimeScope LifetimeScope { get; init; }

    public async Task InvokeAsync(Microsoft.AspNetCore.Connections.ConnectionContext context, ConnectionDelegate callback)
    {
        var connectionContainer = LifetimeScope.BeginLifetimeScope(context, builder =>
        {
            builder.RegisterInstance(context).AsSelf().ExternallyOwned();
            builder.RegisterType<KestrelConnectionContext>().AsSelf().SingleInstance();
        });

        var uContext = connectionContainer.Resolve<KestrelConnectionContext>();

        await callback.Invoke(uContext);
    }
}
