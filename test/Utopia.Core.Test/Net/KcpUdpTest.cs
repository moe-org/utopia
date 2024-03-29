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

    private static async Task TestFor(byte[] data)
    {
        var (sender, receiver) = GetSockets();

        await SocketTestSlot.TestFor(sender, receiver, data, null);
    }

    [Fact]
    public async Task KcpUdpWriteAndReadTest()
    {
        var bytes = Enumerable.Repeat((byte)10, 512).ToArray();

        await TestFor(bytes);
    }

    [Fact]
    public async void UdpWriteAndReadMultipleThreadTest()
    {
        // preapre five tasks
        Task[] tasks =
        [
            TestFor(generate(0)),
            TestFor(generate(1)),
            TestFor(generate(2)),
            TestFor(generate(3)),
            TestFor(generate(4)),
        ];
        byte[] generate(byte index)
        {
            return Enumerable.Repeat(index, 512).ToArray();
        }

        await Task.WhenAll(tasks);
    }
}
