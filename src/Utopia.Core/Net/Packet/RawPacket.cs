// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MemoryPack;
using Microsoft.AspNetCore.Http.Features;

namespace Utopia.Core.Net.Packet;

[MemoryPackable]
public sealed partial class RawPacket
{
    [MemoryPackInclude]
    private string Guuid { get; set; } = string.Empty;

    [MemoryPackInclude]
    public ReadOnlySequence<byte> Data { get; set; } = new();

    [MemoryPackIgnore]
    public Guuid ID { get; set; }

    [MemoryPackOnSerializing]
    private void CovertTo()
    {
        Guuid = ID.ToString();
    }

    [MemoryPackOnDeserialized]
    private void CovertFrom()
    {
        ID = Core.Guuid.Parse(Guuid);
    }

    [MemoryPackConstructor]
    public RawPacket()
    {

    }

    public RawPacket(Guuid ID, ReadOnlySequence<byte> data)
    {
        Guuid = ID.ToString();
        Data = data;
        this.ID = ID;
    }
}
