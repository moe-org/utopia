// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features;

namespace Utopia.Core.Net;

public abstract class ConnectionContext
{
    /// <summary>
    /// 获取特征集合
    /// </summary>
    public IFeatureCollection Features { get; }

    /// <summary>
    /// 应用程序请求上下文
    /// </summary>
    /// <param name="features"></param>
    public ConnectionContext(IFeatureCollection features)
    {
        this.Features = new FeatureCollection(features);
    }
}
