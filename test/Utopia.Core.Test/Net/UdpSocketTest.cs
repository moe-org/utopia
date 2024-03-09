#region

using System.Diagnostics;
using System.Net;
using Autofac;
using Microsoft.Extensions.Logging;
using Utopia.Core.Net;
using Xunit.Abstractions;

#endregion

namespace Utopia.Core.Test.Net;

[Collection("UDP Tests")]
public class UdpSocketTest(ITestOutputHelper helper)
{
    public IContainer container = ContainerManager.GetContainerWithLogger(helper);

    private static readonly Random s_rnd = new();

    public static (
        (IPEndPoint local, IPEndPoint remote),
        (IPEndPoint local, IPEndPoint remote)) Create()
    {
        var one = s_rnd.Next(1024, short.MaxValue);
        var two = s_rnd.Next(1024, short.MaxValue);

        return new ValueTuple<(IPEndPoint local, IPEndPoint remote), (IPEndPoint local, IPEndPoint remote)>(
            new ValueTuple<IPEndPoint, IPEndPoint>(
                new IPEndPoint(IPAddress.Parse("127.0.0.1"), one),
                new IPEndPoint(IPAddress.Parse("127.0.0.1"), two)),
            new ValueTuple<IPEndPoint, IPEndPoint>(
                new IPEndPoint(IPAddress.Parse("127.0.0.1"), two),
                new IPEndPoint(IPAddress.Parse("127.0.0.1"), one)));
    }

    public static (UDPSocket, UDPSocket) GetSockets()
    {
        var (client, server) = Create();

        var logger = ContainerManager.Container.Value.Resolve<ILogger<UDPSocket>>();
        var manager = ContainerManager.Container.Value.Resolve<GlobalUDPManager>();

        return new ValueTuple<UDPSocket, UDPSocket>(
            new UDPSocket(client.local, client.remote, logger, manager),
            new UDPSocket(server.local, server.remote, logger, manager));
    }

    private static async Task TestFor(byte[] data, IContainer container)
    {
        var logger = container.Resolve<ILogger<UdpSocketTest>>();

        var (sender, receiver) = GetSockets();

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
                logger.LogDebug("UDP Socket Receive {} bytes", got);
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

    [Fact]
    public async void UdpWriteAndReadTest()
    {
        var bytes = Enumerable.Repeat((byte)10, 512).ToArray();
        await TestFor(bytes, container);
    }
}
