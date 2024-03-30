// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Buffers;
using System.Net;
using Autofac;
using Autofac.Core.Lifetime;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using Utopia.Core.Net.Packet;

namespace Utopia.Core.Net;

public class KestrelConnectionHandler : ConnectionHandler
{
    public required Logger<KestrelConnectionHandler> Logger { get; init; }

    public required ILifetimeScope LifetimeScope { get; init; }

    public required KestrelConnectionContext Context { get; init; }

    public required TaskFactory PacketDispatchTaskFactory { get; init; }

    public override async Task OnConnectedAsync(Microsoft.AspNetCore.Connections.ConnectionContext connection)
    {
        try
        {
            var input = connection.Transport.Input;
            var token = connection.ConnectionClosed;
            // process
            while (!token.IsCancellationRequested)
            {
                // read length
                int length;
                {
                    var result = await input.ReadAtLeastAsync(sizeof(int), token);

                    if (!result.IsCompleted)
                    {
                        break;
                    }

                    var buf = new byte[sizeof(int)];

                    result.Buffer.Slice(0, 4).CopyTo(buf);

                    length = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buf));

                    input.AdvanceTo(result.Buffer.GetPosition(sizeof(int)));
                }
                // read data
                // ReadOnlySequence<byte> sequence = new();
                byte[] buffer = new byte[length];
                {
                    int read = 0;
                    while (read != length)
                    {
                        var result = await input.ReadAsync(token);

                        if (!result.IsCompleted)
                        {
                            break;
                        }

                        if (result.Buffer.Length == 0)
                        {
                            continue;
                        }

                        int got = Math.Min((int)result.Buffer.Length, read);

                        read += got;

                        var buf = result.Buffer.Slice(0, got);

                        buf.CopyTo(buffer);

                        input.AdvanceTo(buf.GetPosition(got));
                    }
                }

                // parse
                {
                    var sequence = new ReadOnlySequence<byte>(buffer);
                    var pack = MemoryPack.MemoryPackSerializer.Deserialize<SinglePacket>(sequence);

                    if (pack == null)
                    {
                        throw new InvalidDataException("MemoryPackSerializer.Deserialize<SinglePacket>() returns null");
                    }

                    var id = Guuid.Parse(pack.Guuid);

                    var packet = Context.Packetizer.ConvertPacket(id, sequence);

                    // dispatch
                    _ = PacketDispatchTaskFactory.StartNew(async () =>
                    {
                        var result = await Context.Dispatcher.DispatchPacket(id, packet);

                        if (!result)
                        {
                            Logger.LogError("Packet with Guuid {guuid} has no dispatcher", id);
                        }
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "get an exception");
        }
        finally
        {
            await connection.DisposeAsync();
        }
    }


}
