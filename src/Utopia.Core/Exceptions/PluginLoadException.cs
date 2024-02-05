#region

using Utopia.Core.Plugin;

#endregion

namespace Utopia.Core.Exceptions;

public class PluginLoadException : Exception
{
    public PluginLoadException(
        IPluginDependencyInformation information,
        string msg,
        Exception? inner = null) : base(msg, inner)
    {
        this.Plugin = information;
    }

    public IPluginDependencyInformation Plugin { get; init; }
}
