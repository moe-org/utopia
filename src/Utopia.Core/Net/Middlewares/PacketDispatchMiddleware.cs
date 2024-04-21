// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using Autofac;
using Autofac.Core.Lifetime;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using Utopia.Core.Net.Packet;
using static Utopia.Core.Net.IMiddleware;

namespace Utopia.Core.Net.Middlewares;

public class PacketDispatchMiddleware : IMiddleware
{
    public required ILogger<PacketDispatchMiddleware> Logger { get; init; }

    public async Task InvokeAsync(KestrelConnectionContext context, UtopiaConnectionDelegate next)
    {
        while (context.PacketToDispatch.Reader.TryRead(out var packet))
        {
            var result = await context.Dispatcher.DispatchPacket(new(context), packet.ID, packet.Obj);

            if (!result)
            {
                Logger.LogError("Packet with Guuid {guuid} has no dispatcher", packet.ID);
            }
        }

        await next(context);
    }
}
