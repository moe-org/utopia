#region

using System.Reflection;
using Autofac;
using Microsoft.Extensions.Logging;
using Utopia.Core.Exceptions;

#endregion

namespace Utopia.Core.Plugin;

/// <summary>
///     插件加载器
/// </summary>
public class PluginLoader<PluginT> : IPluginLoader<PluginT> where PluginT : IPluginBase
{
    private readonly WeakThreadSafeEventSource<ContainerBuilder> _event = new();

    protected readonly List<PluginT> _loadedPlugins = [];

    protected readonly object _lock = new();
    public required ILifetimeScope Container { protected get; init; }

    public required ILogger<PluginLoader<PluginT>> Logger { protected get; init; }

    public IEnumerable<PluginT> LoadedPlugins
    {
        get
        {
            lock (this._lock)
            {
                return this._loadedPlugins;
            }
        }
    }

    public event Action<ContainerBuilder> ActivatingPlugin
    {
        add => this._event.Register(value);
        remove => this._event.Unregister(value);
    }

    public void Activate(IEnumerable<IUnloadedPlugin> plugins)
    {
        var sortedPlugins = this.SortByDependencies(plugins);

        foreach (var plugin in sortedPlugins)
        {
            var types = plugin.Load();

            foreach (var type in types)
            {
                var instance = this.ActivatePlugin(type);

                lock (this._lock)
                {
                    this._loadedPlugins.Add(instance);
                }
            }
        }
    }

    protected IEnumerable<IUnloadedPlugin> SortByDependencies(IEnumerable<IUnloadedPlugin> plugins)
    {
        var allPlugins = plugins.ToDictionary(k => k.Info.Id);
        Dictionary<IUnloadedPlugin, bool> searching = [];
        List<IUnloadedPlugin> result = [];

        void search(IUnloadedPlugin plugin)
        {
            if (searching.TryGetValue(plugin, out var value))
            {
                if (value)
                    throw new PluginLoadException(plugin.Info, "recycle dependency detected");
                // the plugin was resolved
                return;
            }

            searching[plugin] = true;

            foreach (var deps in plugin.Info.Dependencies)
                try
                {
                    search(allPlugins![deps.Item1]);
                }
                catch (Exception e)
                {
                    throw new PluginLoadException(plugin.Info, "get an error when resolve the dependencies", e);
                }

            result.Add(plugin);
            searching[plugin] = false;
        }

        foreach (var plugin in plugins) search(plugin);

        return result;
    }

    protected PluginT ActivatePlugin(Type type)
    {
        var container = this.Container.BeginLifetimeScope(builder =>
        {
            builder
                .RegisterType(type)
                .As<PluginT>()
                .SingleInstance();

            // call ContainerBuildAttribute
            var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Where(method => method.GetCustomAttribute<ContainerBuildAttribute>() != null);

            foreach (var method in methods) method.Invoke(null, [builder]);

            this._event.Fire(builder, false);
        });

        try
        {
            return container.Resolve<PluginT>();
        }
        catch
        {
            container.Dispose();
            throw;
        }
    }
}