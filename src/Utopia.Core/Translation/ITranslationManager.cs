#region

using System.Diagnostics.CodeAnalysis;

#endregion

namespace Utopia.Core.Translation;

/// <summary>
///     A translation manager allow you to get a <see cref="ITranslationProvider" />.
///     This class help you update translation from another plugin so that you need not update the origin plugin.
/// </summary>
public interface ITranslationManager
{
    void AddTranslationProvider(ITranslationProvider provider);

    /// <summary>
    ///     Try get translation in the list.
    ///     This will check the <see cref="ITranslationProvider" /> in from the beginning to the end.
    /// </summary>
    /// <param name="language">要获取的翻译语言</param>
    /// <param name="translateProviderId">
    ///     翻译提供者ID，如果为null，则在所有翻译提供者中搜索翻译条目。
    ///     如果多个翻译提供者提供相同的翻译条目，返回结果不定。
    /// </param>
    /// <param name="item">翻译条目ID</param>
    /// <param name="result">获取到的结果，如果获取失败，设置为null。</param>
    /// <returns>如果获取成功，返回true。</returns>
    bool TryGetTranslation(LanguageID language, string item, [NotNullWhen(true)] out string? result);
}
