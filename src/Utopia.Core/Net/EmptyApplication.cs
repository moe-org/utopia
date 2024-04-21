// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Features;

namespace Utopia.Core.Net;

public class EmptyApplication : IHttpApplication<string>
{
    public string CreateContext(IFeatureCollection contextFeatures)
    {
        return string.Empty;
    }

    public void DisposeContext(string context, Exception? exception)
    {

    }

    public Task ProcessRequestAsync(string context)
    {
        return Task.CompletedTask;
    }
}
