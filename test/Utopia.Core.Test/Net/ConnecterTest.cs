#region

using System.Buffers;
using Autofac;
using Microsoft.Extensions.Logging;
using Moq;
using Utopia.Core.Net;

#endregion

namespace Utopia.Core.Test.Net;

public class ConnecterTest
{
    [Fact]
    public async void ConnectHandlerTest()
    {
        // prepare
        var (client, server) = PipeSocket.Create();

        using var clientHandler = new ConnectHandler(client)
        {
            Logger = ContainerManager.Container.Value.Resolve<ILogger<ConnectHandler>>(),
            Container = null!
        };
        using var serverHandler = new ConnectHandler(server)
        {
            Logger = ContainerManager.Container.Value.Resolve<ILogger<ConnectHandler>>(),
            Container = null!
        };

        var packet = new Guuid("root", "packet");
        var data = (byte[]) [1, 1, 4, 5, 1, 4];

        Mock<IPacketFormatter> formatter = new();

        formatter.Setup(f => f.GetValue(It.IsAny<Guuid>(), It.IsAny<ReadOnlySequence<byte>>()))
            .Returns((Guuid _, ReadOnlySequence<byte> a) => { return a.ToArray(); });
        formatter.Setup(f => f.ToPacket(It.IsAny<Guuid>(), It.IsAny<object>()))
            .Returns((Guuid id, object a) =>
            {
                Assert.Equal(packet, id);
                return (Memory<byte>)(byte[])a;
            });

        serverHandler.Packetizer.Formatters.TryAdd(packet, formatter.Object);
        clientHandler.Packetizer.Formatters.TryAdd(packet, formatter.Object);
        var received = false;

        Mock<IPacketHandler> handler = new();
        handler.Setup(h => h.Handle(It.IsAny<Guuid>(), It.IsAny<object>()))
            .Returns((Guuid id, object rev) =>
            {
                Assert.Equal(packet, id);
                if (((byte[])rev).SequenceEqual(data)) received = true;
                return Task.CompletedTask;
            });

        serverHandler.Dispatcher.Handlers.TryAdd(packet, handler.Object);

        // write
        Thread.Sleep(500);

        Assert.True(serverHandler.Running);
        Assert.True(clientHandler.Running);

        clientHandler.WritePacket(packet, data);

        Thread.Sleep(500);

        clientHandler.Disconnect();
        serverHandler.Disconnect();

        // check
        Assert.True(received);

        Assert.False(serverHandler.Running);
        Assert.False(clientHandler.Running);
    }
}