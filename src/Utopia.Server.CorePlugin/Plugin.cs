// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using Utopia.Core;

namespace Utopia.Server.CorePlugin;

public partial class Plugin : IPlugin
{
    private bool _disposed = false;

    private WeakThreadSafeEventSource _source = new();

    public event EventHandler PluginDeactivated
    {
        add
        {
            _source.Register(value);
        }
        remove
        {
            _source.Unregister(value);
        }
    }

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

        if (disposing)
        {
            _source.Fire(this);
        }

        _disposed = true;
    }
}
