#region

using Range = SemanticVersioning.Range;
using Version = SemanticVersioning.Version;

#endregion

namespace Utopia.Core.Plugin;

/// <summary>
///     插件的依赖信息.这些信息往往在加载插件(所在的dll)之前就需要get到.
/// </summary>
public interface IPluginDependencyInformation
{
    /// <summary>
    ///     插件自己的id
    /// </summary>
    public Guuid Id { get; }

    /// <summary>
    ///     版本号
    /// </summary>
    public Version Version { get; }

    /// <summary>
    ///     依赖的插件的guuid以及版本范围
    /// </summary>
    public IEnumerable<(Guuid, Range)> Dependences { get; }
}