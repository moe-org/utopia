// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Utopia.Core.Net;
using Autofac;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging;

namespace Utopia.Core.Test.Net;
public class UdpSocketTest
{
    private (
        (Socket,IPEndPoint local, IPEndPoint remote),
        (Socket,IPEndPoint local, IPEndPoint remote)) Create()
    {
        var server = new Socket(AddressFamily.InterNetwork,SocketType.Dgram, ProtocolType.Udp);

        var client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        return new(new(
            client,
            new IPEndPoint(IPAddress.Loopback, 456),
            new IPEndPoint(IPAddress.Loopback, 345)),
            new(
            server,
            new IPEndPoint(IPAddress.Loopback, 345),
            new IPEndPoint(IPAddress.Loopback, 456)));
    }

    private async Task<(UDPSocket, UDPSocket)> GetSockets()
    {
        var (client, server) = Create();

        var logger = ContainerManager.Container.Value.Resolve<ILogger<UDPSocket>>();
        var manager = ContainerManager.Container.Value.Resolve<GlobalUDPManager>();

        return new(
            new UDPSocket(client.Item1, client.local,client.remote,logger, manager),
            new UDPSocket(server.Item1, server.local,server.remote, logger, manager));
    }

    [Fact]
    public async void UdpTest()
    {
        var (sender, receiver) = await GetSockets();

        Assert.True(sender.Alive);
        Assert.True(receiver.Alive);

        Thread.Sleep(100);

        await sender.Write((byte[])[1, 1, 4]);
        await sender.Write((byte[])[5, 1, 4]);

        Thread.Sleep(100);

        // read
        MemoryStream memoryStream = new(6);
        byte[] buffer = new byte[6];

        while(memoryStream.Length != 6)
        {
            var got = await receiver.Read(buffer);

            memoryStream.Write(buffer,0, got);
        }

        Assert.Equal([1,1,4,5,1,4],memoryStream.ToArray());

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
