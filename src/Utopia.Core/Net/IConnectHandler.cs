#region

using Autofac;

#endregion

namespace Utopia.Core.Net;

/// <summary>
///     这个类处理了对于<see cref="ISocket" />的输入/输出.
///     把数据传输提升到了object而不是byte[]的层面.
///     使用<see cref="IPacketizer" />,<see cref="IDispatcher" />等接口对包进行处理.
/// </summary>
public interface IConnectHandler : IDisposable
{
    /// <summary>
    ///     链接是否仍然存活.
    ///     一次返回false之后应该不再返回true.(即链接断开后对于此IConnectHandler不再次重连)
    ///     <see cref="Disconnect" />
    /// </summary>
    bool Running { get; }

    /// <summary>
    ///     包分发器
    /// </summary>
    IDispatcher Dispatcher { get; }

    /// <summary>
    ///     包格式化器
    /// </summary>
    IPacketizer Packetizer { get; }

    /// <summary>
    ///     这个链接的容器.
    /// </summary>
    ILifetimeScope Container { get; }

    /// <summary>
    ///     Write a packet,but it wont covert the packet to bytes.You should do it first.
    ///     Or see <see cref="WritePacket(Guuid, object)" />.
    /// </summary>
    /// <param name="packetTypeId"></param>
    /// <param name="obj"></param>
    void WritePacket(Guuid packetTypeId, object obj);

    /// <summary>
    ///     正常地断开链接.
    ///     调用此函数后<see cref="Running" />应该返回false.
    /// </summary>
    void Disconnect();

    /// <summary>
    ///     链接断开事件.如果触发了某些异常,则参数为此异常.
    /// </summary>
    event Action<Exception?> ConnectionClosed;
}