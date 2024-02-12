namespace Utopia.Core.Plugin;

/// <summary>
///     插件的运行时信息,供人类读取.
/// </summary>
public interface IPluginInformation : IPluginDependencyInformation
{
    /// <summary>
    ///     人类可读的名称
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     人类可读的描述
    /// </summary>
    string Description { get; }

    /// <summary>
    ///     许可协议
    /// </summary>
    string License { get; }

    /// <summary>
    ///     网址，或者其他联系方式等。
    /// </summary>
    string Homepage { get; }

    /// <summary>
    /// 作者
    /// </summary>
    string Author { get; }
}
