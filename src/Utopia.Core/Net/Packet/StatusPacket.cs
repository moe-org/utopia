// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MemoryPack;

namespace Utopia.Core.Net.Packet;

[MemoryPackable]
public partial class StatusPacket : IWithPacketId
{
    public static Guuid PacketID => InternalHelper.NewInternalGuuid("Net", "Packet", "Status");

    public string? LoginAs { get; set; } = null;

    public string Version { get; set; } = VersionUtility.UtopiaCoreVersion.ToString();
}

