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

/// <summary>
/// 服务器或者客户端可以发送此包。
/// 发送方发送过后继续运行。
/// 接收方接受到此包后应该提示用户并断开连接。
/// </summary>
[MemoryPackable]
public partial class ErrorPacket : IWithPacketId
{
    public static Guuid PacketID => InternalHelper.NewInternalGuuid("Net", "Packet", "Error");

    public string ErrorMessage { get; set; } = string.Empty;
}

