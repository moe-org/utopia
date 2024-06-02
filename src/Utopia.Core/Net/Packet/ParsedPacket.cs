// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utopia.Core.Net.Packet;
public sealed class ParsedPacket
{
    public Guuid ID { get; set; }

    public object Obj { get; set; }

    public ParsedPacket(Guuid ID, object Obj)
    {
        ArgumentNullException.ThrowIfNull(Obj);
        this.ID = ID;
        this.Obj = Obj;
    }
}
