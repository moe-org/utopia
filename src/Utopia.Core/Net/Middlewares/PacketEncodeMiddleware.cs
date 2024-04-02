// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utopia.Core.Net.Middlewares;
public class PacketEncodeMiddleware : IMiddleware
{
    public async Task InvokeAsync(KestrelConnectionContext context, IMiddleware.UtopiaConnectionDelegate next)
    {
        await foreach (var packet in context.PacketToSend.Reader.ReadAllAsync(context.ConnectionClosed))
        {
            var parsed = context.Packetizer.Encode(packet.ID, packet.Obj);

            await context.PacketToWrite.Writer.WriteAsync(new(packet.ID, new(parsed)));
        }

        await next(context);
    }
}
