#region

using System.Globalization;

#endregion

namespace Utopia.Core.Translation;

/// <summary>
///     翻译唯一标识符号，用于标识一个翻译。
/// </summary>
public sealed class LanguageID
{
    /// <summary>
    ///     ISO 639-1所指定的两位字母语言编码。
    /// </summary>
    public readonly string Language;

    /// <summary>
    ///     ISO 3166-1所指定的两位字母地区编码。
    /// </summary>
    public readonly string Location;

    /// <summary>
    ///     构造一个语言标识条目
    /// </summary>
    /// <param name="language">ISO 639-1标准语言代码(Two letter)</param>
    /// <param name="location">ISO 3166-1标准地区代码(Two letter)</param>
    /// <exception cref="ArgumentException">如果参数不符合标准。</exception>
    public LanguageID(string language, string location)
    {
        ArgumentNullException.ThrowIfNull(language);
        ArgumentNullException.ThrowIfNull(location);

        if (language.Length != 2 || location.Length != 2)
            throw new FormatException("the length of language or location is not 2");
        if (!(language.All(char.IsLetter) && location.All(char.IsLetter)))
            throw new FormatException("the language or location is not all letter");

        this.Language = language.ToLowerInvariant();
        this.Location = location.ToLowerInvariant();
    }

    /// <summary>
    ///     从<see cref="CultureInfo" />和<see cref="RegionInfo" />中读取环境值并生成一个LanguageID.
    /// </summary>
    public static LanguageID GetDefault()
    {
        return new LanguageID(
            CultureInfo.CurrentCulture.TwoLetterISOLanguageName,
            RegionInfo.CurrentRegion.TwoLetterISORegionName);
    }

    /// <summary>
    ///     Can parse [LANGUAGE][separator][LOCATION].
    ///     [separator] can be '-' or '_' or ' '(SPACE).
    ///     [LANGUAGE] and [LOCATION] obey the ISO 639-1 and ISO 3166-1 standard(Two letter version).
    /// </summary>
    public static LanguageID Parse(string id)
    {
        return !TryParse(id, out var identifence)
            ? throw new FormatException("the format of TranslateIdentifence is invalid")
            : identifence!;
    }

    /// <summary>
    ///     see <see cref="Parse(string)" />
    /// </summary>
    public static bool TryParse(string id, out LanguageID? result)
    {
        ArgumentNullException.ThrowIfNull(id);

        var parts = id.Split('_', '-', ' ');

        if (parts.Length != 2)
        {
            result = null;
            return false;
        }

        result = new LanguageID(parts[0], parts[1]);
        return true;
    }

    public override string ToString()
    {
        return this.Language + "_" + this.Location;
    }

    public override int GetHashCode()
    {
        return StringComparer.Ordinal.GetHashCode(ToString());
    }

    public override bool Equals(object? obj)
    {
        if (obj == null) return false;
        if (obj.GetType().IsAssignableFrom(this.GetType()))
        {
            var o = (LanguageID)obj;
            return o.Language.Equals(this.Language) && o.Location.Equals(this.Location);
        }

        return false;
    }

    public static bool operator ==(LanguageID left, LanguageID right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(LanguageID left, LanguageID right)
    {
        return !(left == right);
    }
}
