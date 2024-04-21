// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utopia.Core.Net.Middlewares;
public class PacketDecodeMiddleware : IMiddleware
{
    public async Task InvokeAsync(KestrelConnectionContext context, IMiddleware.UtopiaConnectionDelegate next)
    {
        while (context.PacketToParse.Reader.TryRead(out var packet))
        {
            var parsed = context.Packetizer.Decode(packet.ID, packet.Data);

            await context.PacketToDispatch.Writer.WriteAsync(new(packet.ID, parsed));
        }

        await next(context);
    }
}
