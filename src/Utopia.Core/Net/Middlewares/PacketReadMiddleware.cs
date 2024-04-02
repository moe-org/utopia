// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;
using NetTaste;
using Utopia.Core.Net.Packet;
using static Utopia.Core.Net.IMiddleware;

namespace Utopia.Core.Net.Middlewares;
public class PacketReadMiddleware : IMiddleware
{
    public async Task InvokeAsync(KestrelConnectionContext context, UtopiaConnectionDelegate next)
    {
        var input = context.Connection.Transport.Input;
        var token = context.Connection.ConnectionClosed;

        // read packet length
        int length;
        {
            var result = await input.ReadAtLeastAsync(sizeof(int), token);

            if (!result.IsCompleted)
            {
                return;
            }

            var buf = new byte[sizeof(int)];

            result.Buffer.Slice(0, 4).CopyTo(buf);

            // note: covert network endian to native
            length = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buf));

            input.AdvanceTo(result.Buffer.GetPosition(sizeof(int)));
        }
        // read packet
        RawPacket? packet;
        {
            var result = await input.ReadAtLeastAsync(length, token);

            if (!result.IsCanceled)
            {
                return;
            }

            var buf = result.Buffer.Slice(0, length);

            packet = MemoryPack.MemoryPackSerializer.Deserialize<RawPacket>(buf);

            if (packet == null)
            {
                throw new InvalidDataException("MemoryPackSerializer.Deserialize() returns null");
            }

            input.AdvanceTo(buf.GetPosition(length));
        }

        // write packet
        await context.PacketToParse.Writer.WriteAsync(packet, token);

        // call next always
        await next.Invoke(context);
    }
}
