// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utopia.Core.Net;

namespace Utopia.Core.Test.Net;

public class PipeSocketTest
{
    [Fact]
    public async void FakeSocketTransmitTest()
    {
        var (sender, receiver) = PipeSocket.Create();

        Assert.True(sender.Alive);
        Assert.True(receiver.Alive);

        await sender.Write((byte[])[1, 1, 4]);
        await sender.Write((byte[])[5, 1, 4]);

        sender.Shutdown();
        Assert.False(sender.Alive);

        byte[] buf = new byte[2];

        var read = await receiver.Read(buf);

        Assert.Equal(2, read);
        Assert.Equal(buf, (byte[])[1, 1]);

        read = await receiver.Read(buf);

        Assert.Equal(2, read);
        Assert.Equal(buf, (byte[])[4, 5]);

        read = await receiver.Read(buf);

        Assert.Equal(2, read);
        Assert.Equal(buf, (byte[])[1, 4]);

        receiver.Shutdown();
        Assert.False(receiver.Alive);

        // send and no receive
        await sender.Write((byte[])[1, 1]);
        Thread.Sleep(10);
        var received = await receiver.Read(new byte[2]);

        Assert.Equal(0, received);
    }
}
