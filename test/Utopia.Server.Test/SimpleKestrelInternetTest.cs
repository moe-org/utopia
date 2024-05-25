// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MemoryPack;
using Utopia.Core.Net.Packet;
using Utopia.Core.Net;
using Xunit.Abstractions;

namespace Utopia.Server.Test;

public class SimpleKestrelInternetTest(ITestOutputHelper output) : IntegrationTest(output)
{
    protected override void AfterRun()
    {

    }

    protected override void BeforeRun()
    {

    }

    protected override void Configure()
    {

    }

    [Fact]
    public void ErrorPacketTest()
    {
        using Socket socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(new IPAddress(new byte[] { 127, 0, 0, 1 }), Options.Port);

        // construct error packet
        var errorPacket = new ErrorPacket()
        {
            ErrorMessage = "this is a test error packet"
        };

        var obj = (new MemoryPackPacketFormatter<ErrorPacket>()).ToPacket(ErrorPacket.PacketID, errorPacket);

        var rawPacket = MemoryPackSerializer.Serialize<RawPacket>(new()
        {
            Data = new(obj),
            ID = ErrorPacket.PacketID
        });

        var length = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(rawPacket.Length));

        using MemoryStream buf = new();
        buf.Write(length);
        buf.Write(rawPacket);

        socket.Send(buf.ToArray());

        Thread.Sleep(200);

        // should fail after send a error packet
        Assert.Throws<SocketException>(() =>
        {
            socket.Send((byte[])[0]);
        });

        StopAndWait();

        // check the log 
        Assert.Contains(Launcher.Logs.ToArray().Select((log) => log.message), (s) => s.Contains("this is a test error packet"));
    }
}
