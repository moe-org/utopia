// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Utopia.Core.Net;

namespace Utopia.Godot.src.Net;

public enum ConnectionType
{
    Tcp,
    UdpKcp
}

/// <summary>
/// 连接器
/// </summary>
public class Connector
{
    public ISocket Socket { get; private set; } = null!;

    public IConnectionHandler ConnectionHandler { get; private set; } = null!;

    public required GlobalUDPManager UdpManager { get; init; }

    public required ILogger<UDPSocket> Logger { get; init; }

    public void Connect(string address,int port,ConnectionType type)
    {
        if(type == ConnectionType.Tcp)
        {
            var tcp = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            tcp.Connect(address, port);

            Socket = new StandardSocket(tcp);
        }
        if(type == ConnectionType.UdpKcp)
        {
            var tcp = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Udp);

            tcp.Connect(address, port);

            Socket = new KcpSocket(new UDPSocket(
                new IPEndPoint(IPAddress.Any, 0),
                new IPEndPoint(IPAddress.Parse(address),port),
                Logger,
                UdpManager));
        }

    }
}
