// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features;
using Utopia.Core.Mnagement;
using Utopia.Core.Net.Packet;

namespace Utopia.Core.Net;

public class ConnectionContext(KestrelConnectionContext context)
{
    public string ConnectionId { get; } = context.Connection.ConnectionId;

    public ChannelWriter<ParsedPacket> PacketWriter { get; } = context.PacketToSend.Writer;

    public User? User
    {
        get
        {
            return context.user;
        }
        set
        {
            context.user = value;
        }
    }

    public IDispatcher Dispatcher => context.Dispatcher;

    public IPacketizer Packetizer => context.Packetizer;

    public ConcurrentDictionary<object,object?> Items => new();

    public CancellationToken ConnectionClosed => context.ConnectionClosed;
}
