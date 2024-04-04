// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Diagnostics;
using MemoryPack;
using Utopia.Core.Net.Packet;

namespace Utopia.Core.Net;

public class MemoryPackPacketFormatter<T> : IPacketFormatter where T : IWithPacketId
{
    public object GetValue(Guuid packetId, ReadOnlySequence<byte> packet)
    {
        if(packetId != T.PacketID)
        {
            throw new ArgumentException($"this formatter can only process `{T.PacketID}` packet but get a `{packetId}` packet");
        }

        var obj = MemoryPackSerializer.Deserialize<T>(packet);

        return obj == null ? throw new InvalidOperationException("MemoryPackSerizlizer.Deserialize<T>() returns null") : obj;
    }

    public Memory<byte> ToPacket(Guuid packetId, object value)
    {
        if(packetId != T.PacketID)
        {
            throw new ArgumentException($"this formatter can only process `{T.PacketID}` packet but get a `{packetId}` packet");
        }

        Guard.IsNotNull(value);
        Guard.IsAssignableToType(value, typeof(T));

        return MemoryPackSerializer.Serialize((T)value);
    }
}

