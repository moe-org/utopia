// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using Utopia.Core;
using Range = SemanticVersioning.Range;
using Version = SemanticVersioning.Version;

namespace Utopia.Server.CorePlugin;

public sealed class Plugin : IPlugin
{
    private bool _disposed = false;

    public Guuid Id { get; }

    public Version Version { get; }

    public IEnumerable<(Guuid, Range)> Dependencies { get; }

    public string Name { get; }

    public string Description { get; }

    public string License { get; }

    public string Homepage { get; }

    public event Action? PluginDeactivated;

    ~Plugin()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
    }

}
