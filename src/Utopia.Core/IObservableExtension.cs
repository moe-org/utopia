// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utopia.Core;
public interface IObservableExtension<out T> : IObservable<T>
{
    /// <summary>
    /// 上一次推送的值.即最新值.
    /// </summary>
    T LastUpdateValue { get; }
}
