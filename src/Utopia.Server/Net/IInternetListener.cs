// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System.Net;
using System.Net.Sockets;
using Utopia.Core.Net;

namespace Utopia.Server.Net;

public class SocketAcceptEvent(ISocket socket)
{
    private ISocket _socket = socket;

    public ISocket Socket
    {
        get => _socket;
        set {
            ArgumentNullException.ThrowIfNull(value);

            this._socket = value;
        }
    }
}

public class TcpAcceptEvent(Socket socket, ISocket initSocket) : SocketAcceptEvent(initSocket)
{
    public Socket TcpSocket { get; } = socket;
}

/// <summary>
/// 网络服务器接口，线程安全。负责接受链接。
/// </summary>
public interface IInternetListener : IDisposable
{
    /// <summary>
    /// 目前正在监听的端口号。如果服务器尚未启动则引发异常。
    /// </summary>
    public int Port { get; }

    /// <summary>
    /// 检查网络服务器是否处在工作状态
    /// </summary>
    public bool Working { get; }

    /// <summary>
    /// Accept到了一个新的链接，Socket为TCP，EndPoint为UDP，两者不可能同时置空
    /// </summary>
    public event Action<SocketAcceptEvent> AcceptEvent;

    /// <summary>
    /// 链接并监听端口，一个INetServer只能监听一个端口。
    /// </summary>
    /// <param name="port">端口号</param>
    void Listen(int port);

    /// <summary>
    /// 接受新链接，必须监听一个端口号后才能调用。
    /// </summary>
    /// <returns></returns>
    Task<ISocket> Accept();

    /// <summary>
    /// 停机，释放资源。等价于调用<see cref="IDisposable.Dispose"/>
    /// </summary>
    void Shutdown();
}
