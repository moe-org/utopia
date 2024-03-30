#region

using System.Net;

#endregion

namespace Utopia.Core.Net;

/// <summary>
///     代表一个用于通讯的套接字接口.
/// </summary>
public interface ISocket : IDisposable
{
    /// <summary>
    ///     具有意义的链接是否存活flag.
    ///     在返回false之后应该不再返回true(即链接断开后不再重新链接).
    ///     实现应该使用Ping等多种手段确保Socket可通信.
    /// </summary>
    bool Alive { get; }

    /// <summary>
    ///     If null,the socket may not based on real socket and has no real remote address.
    /// </summary>
    EndPoint? RemoteAddress { get; }

    /// <summary>
    ///     If null,the socket may not based on real socket and has no real remote address.
    /// </summary>
    EndPoint? LocalAddress { get; }

    /// <summary>
    ///     写入数据.
    ///     如果<see cref="Alive" />返回false则忽略操作.
    /// </summary>
    Task Write(ReadOnlyMemory<byte> data);

    /// <summary>
    ///     读取数据.
    ///     如果<see cref="Alive" />返回false则忽略操作,读取字节数设置为0.
    /// </summary>
    /// <param name="dst">目标缓冲区</param>
    /// <returns>读取的字节数</returns>
    Task<int> Read(Memory<byte> dst);

    /// <summary>
    ///     断开连接
    /// </summary>
    void Shutdown();
}
