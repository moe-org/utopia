namespace Utopia.Core.Plugin;

public interface IPluginBase : IPluginInformation, IDisposable
{
    event Action PluginDeactivated;
}
