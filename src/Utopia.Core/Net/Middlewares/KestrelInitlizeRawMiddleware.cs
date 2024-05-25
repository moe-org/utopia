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

    public const string Key = "Utopia Connection Context";

    public async Task InvokeAsync(Microsoft.AspNetCore.Connections.ConnectionContext context, ConnectionDelegate callback)
    {
        if (!context.Items.ContainsKey(Key))
        {
            var connectionContainer = LifetimeScope.BeginLifetimeScope(context, builder =>
            {
                builder.RegisterInstance(context).As<Microsoft.AspNetCore.Connections.ConnectionContext>().ExternallyOwned();
                builder.RegisterType<KestrelConnectionContext>().AsSelf().SingleInstance();
            });

            context.Items[Key] = connectionContainer.Resolve<KestrelConnectionContext>();

            context.ConnectionClosed.Register(connectionContainer.Dispose);
        }

        await callback.Invoke((KestrelConnectionContext)context.Items[Key]!).ConfigureAwait(false);
    }
}
