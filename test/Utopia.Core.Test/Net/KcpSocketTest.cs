#region

using Utopia.Core.Net;

#endregion

namespace Utopia.Core.Test.Net;

public class KcpSocketTest
{
    [Fact]
    public async Task KcpWriteAndReadTest()
    {
        var (client, server) = PipeSocket.Create();

        using var clientKcp = new KcpSocket(client);
        using var serverKcp = new KcpSocket(server);

        Assert.True(clientKcp.Alive);
        Assert.True(serverKcp.Alive);

        await clientKcp.Write((byte[]) [1, 1, 4]);
        await clientKcp.Write((byte[]) [5, 1, 4]);

        MemoryStream output = new();

        while (true)
        {
            var buf = new byte[6];
            var read = await serverKcp.Read(buf);

            output.Write(buf, 0, read);

            if (output.Length >= 6) break;
        }

        Assert.Equal(0, await serverKcp.Read(new byte[1]));
        Assert.Equal(output.ToArray(), [1, 1, 4, 5, 1, 4]);

        client.Shutdown();
        server.Shutdown();

        Assert.False(client.Alive);
        Assert.False(server.Alive);

        client.Dispose();
        server.Dispose();

        Assert.False(client.Alive);
        Assert.False(server.Alive);

        // send and no receive
        await server.Write((byte[]) [1, 1]);
        Thread.Sleep(10);
        var received = await client.Read(new byte[2]);

        Assert.Equal(0, received);
    }
}