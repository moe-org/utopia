// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Utopia.Core.Net;

namespace Utopia.Core.Test.Net;
public class SocketTestSlot
{

    public static async Task TestFor(ISocket sender, ISocket receiver, byte[] data, ILogger? logger)
    {
        Assert.True(sender.Alive);
        Assert.True(receiver.Alive);

        Thread.Sleep(100);

        // equals to the max send buffer size
        // and it equals to the max receive send buffer size
        var sent = data;

        await sender.Write(sent);

        Thread.Sleep(500);

        // read
        MemoryStream memoryStream = new(sent.Length);
        var buffer = new byte[sent.Length];

        while (memoryStream.Length != sent.Length)
        {
            var got = await receiver.Read(buffer);

            if (got != 0)
            {
                logger?.LogDebug("Socket Receive {} bytes", got);
            }

            memoryStream.Write(buffer, 0, got);
        }

        Assert.Equal(sent, memoryStream.ToArray());

        // check
        receiver.Shutdown();
        sender.Shutdown();
        Assert.False(receiver.Alive);
        Assert.False(sender.Alive);

        // send and no receive
        await sender.Write((byte[])[1, 1]);
        Thread.Sleep(10);
        var received = await receiver.Read(new byte[2]);

        Assert.Equal(0, received);
    }

}
