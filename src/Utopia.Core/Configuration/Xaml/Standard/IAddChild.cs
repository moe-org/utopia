// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utopia.Core.Configuration.Xaml.Standard;

public interface IAddChild
{
    void AddChild(object child);
}

public interface IAddChild<T>
{
    void AddChild(T child);
}
