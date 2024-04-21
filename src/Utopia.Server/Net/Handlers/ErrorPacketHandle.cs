// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Utopia.Core.Net;
using Utopia.Core.Net.Packet;

namespace Utopia.Server.Net.Handlers;
public class ErrorPacketHandle : IPacketHandler<ErrorPacket>
{
    public required ILogger<ErrorPacketHandle> Logger { get; init; }

    public Task Handle(ConnectionContext ctx, ErrorPacket packet)
    {
        Logger.LogError("get an error from remote(connection id:{connectionId}):{error}", ctx.ConnectionId, packet.ErrorMessage);

        ctx.Abort();

        return Task.CompletedTask;
    }
}
