#region

using System.Net;
using System.Net.Sockets;
using Utopia.Core.Net;

#endregion

namespace Utopia.Core.Test.Net;

[Collection("TCP Tests")]
public class TcpSocketTest
{
    public static (StandardSocket, StandardSocket) GetSockets()
    {
        var server = new Socket(SocketType.Stream, ProtocolType.Tcp);

        server.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        var port = (server.LocalEndPoint as IPEndPoint)!.Port;
        server.Listen();

        var client = new Socket(SocketType.Stream, ProtocolType.Tcp);

        client.Connect(new IPEndPoint(IPAddress.Loopback, port));

        var clientSocket = new StandardSocket(client);
        var serverSocket = new StandardSocket(server.Accept());

        return new ValueTuple<StandardSocket, StandardSocket>(clientSocket, serverSocket);
    }

    [Fact]
    public async void TcpWriteAndReadTest()
    {
        var (sender, receiver) = GetSockets();

        Assert.True(sender.Alive);
        Assert.True(receiver.Alive);

        await sender.Write((byte[]) [1, 1, 4]);
        await sender.Write((byte[]) [5, 1, 4]);

        sender.Shutdown();
        Assert.False(sender.Alive);

        var buf = new byte[2];

        var read = await receiver.Read(buf);

        Assert.Equal(2, read);
        Assert.Equal(buf, (byte[]) [1, 1]);

        read = await receiver.Read(buf);

        Assert.Equal(2, read);
        Assert.Equal(buf, (byte[]) [4, 5]);

        read = await receiver.Read(buf);

        Assert.Equal(2, read);
        Assert.Equal(buf, (byte[]) [1, 4]);

        receiver.Shutdown();
        Assert.False(receiver.Alive);

        // send and no receive
        await sender.Write((byte[]) [1, 1]);
        Thread.Sleep(10);
        var received = await receiver.Read(new byte[2]);

        Assert.Equal(0, received);
    }
}
