namespace Utopia.Core.Plugin;

/// <summary>
///     The lifecycle of a plugin.
/// </summary>
public enum PluginLifeCycle
{
    /// <summary>
    ///     插件的初始状态.
    /// </summary>
    Activated,

    /// <summary>
    ///     插件已经执行了Unload操作.
    /// </summary>
    Deactivated
}