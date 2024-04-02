// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using Microsoft.AspNetCore.Connections;

namespace Utopia.Core.Net;

public interface IMiddleware
{
    public delegate Task UtopiaConnectionDelegate(KestrelConnectionContext context);

    Task InvokeAsync(KestrelConnectionContext context, UtopiaConnectionDelegate next);
}
