// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using Autofac;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Utopia.Core.Mnagement;
using Utopia.Core.Net.Packet;

namespace Utopia.Core.Net;

/// <summary>
/// A instance per connection.
/// </summary>
public class KestrelConnectionContext : Microsoft.AspNetCore.Connections.ConnectionContext, IAsyncDisposable
{
    public User? user { get; set; } = null;

    public required IDispatcher Dispatcher { get; init; }

    public required IPacketizer Packetizer { get; init; }

    public Channel<RawPacket> PacketToParse { get; init; } = Channel.CreateUnbounded<RawPacket>();

    public Channel<ParsedPacket> PacketToDispatch { get; init; } = Channel.CreateUnbounded<ParsedPacket>();

    public Channel<ParsedPacket> PacketToSend { get; init; } = Channel.CreateUnbounded<ParsedPacket>();

    public Channel<RawPacket> PacketToWrite { get; init; } = Channel.CreateUnbounded<RawPacket>();

    public required Microsoft.AspNetCore.Connections.ConnectionContext Connection { get; init; }

    public required ILifetimeScope LifetimeScope { get; init; }

    // implement interface

    public override IDuplexPipe Transport
    {
        get
        {
            return Connection.Transport;
        }
        set
        {
            Connection.Transport = value;
        }
    }

    public override string ConnectionId
    {
        get
        {
            return Connection.ConnectionId;
        }
        set
        {
            Connection.ConnectionId = value;
        }
    }

    public override IFeatureCollection Features => Connection.Features;

    public override IDictionary<object, object?> Items
    {
        get
        {
            return Connection.Items;
        }
        set
        {
            Connection.Items = value;
        }
    }

    public override async ValueTask DisposeAsync()
    {
        await Connection.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    ~KestrelConnectionContext()
    {
        DisposeAsync().AsTask().Wait();
    }
}
