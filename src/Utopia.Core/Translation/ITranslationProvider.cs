#region

using System.Diagnostics.CodeAnalysis;

#endregion

namespace Utopia.Core.Translation;

/// <summary>
///     负责提供翻译的类.应该确保线程安全.
/// </summary>
public interface ITranslationProvider
{
    /// <summary>
    ///     获取翻译条目
    /// </summary>
    /// <param name="language">目标语言</param>
    /// <param name="id">翻译条目Id</param>
    /// <param name="result">结果，如果条目不存在，返回null</param>
    /// <returns>如果获取成功，找到翻译条目，返回true。</returns>
    bool TryGetItem(LanguageID language, string id, [NotNullWhen(true)] out string? result);

    /// <summary>
    ///     查询编译条目是否存在
    /// </summary>
    /// <param name="language">目标语言</param>
    /// <param name="id">翻译条目id</param>
    /// <returns>如果翻译条目存在，返回true，否则返回false</returns>
    bool Contain(LanguageID language, string id);
}
