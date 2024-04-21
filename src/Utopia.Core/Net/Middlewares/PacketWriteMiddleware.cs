// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Utopia.Core.Net.Packet;

namespace Utopia.Core.Net.Middlewares;

/// <summary>
/// 负责把包编码为二进制输出。
/// </summary>
public class PacketWriteMiddleware : IMiddleware
{
    public async Task InvokeAsync(KestrelConnectionContext context, IMiddleware.UtopiaConnectionDelegate next)
    {
        // write packet
        while (context.PacketToWrite.Reader.TryRead(out var packet))
        {
            var encodedPacket = MemoryPack.MemoryPackSerializer.Serialize(packet);

            // note: covert native endian to network
            var length = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(encodedPacket.Length));

            await context.Transport.Output.WriteAsync(length);
            await context.Transport.Output.WriteAsync(encodedPacket);
        }

        await next(context);
    }
}

