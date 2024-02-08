#region

using Utopia.Core.Net;

#endregion

namespace Utopia.Core.Test.Net;

/// <summary>
///     Test both kcp and udp
/// </summary>
[Collection("UDP Tests")]
public class KcpUdpTest
{
    public static (KcpSocket, KcpSocket) GetSockets()
    {
        var (one, two) = UdpSocketTest.GetSockets();
        return new ValueTuple<KcpSocket, KcpSocket>(new KcpSocket(one), new KcpSocket(two));
    }

    [Fact]
    public async Task KcpUdpWriteAndReadTest()
    {
        var (sender, receiver) = GetSockets();

        Assert.True(sender.Alive);
        Assert.True(receiver.Alive);

        var bytes = Enumerable.Repeat((byte)10, 128).ToArray();

        await sender.Write(bytes);

        // read
        MemoryStream memoryStream = new(bytes.Length);
        var buffer = new byte[bytes.Length];

        while (memoryStream.Length != bytes.Length)
        {
            Assert.True(receiver.Alive);

            var got = await receiver.Read(buffer);

            memoryStream.Write(buffer, 0, got);
        }

        Assert.Equal(bytes, memoryStream.ToArray());

        // check
        receiver.Shutdown();
        sender.Shutdown();
        Assert.False(receiver.Alive);
        Assert.False(sender.Alive);

        // send and no receive
        await sender.Write((byte[]) [1, 1]);
        Thread.Sleep(10);
        var received = await receiver.Read(new byte[2]);

        Assert.Equal(0, received);
    }
}
