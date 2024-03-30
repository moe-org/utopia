#region

using System.Collections.Frozen;
using System.Xml.Serialization;

#endregion

namespace Utopia.Core.Translation;

public record TranslationDeclareItem(string Text, string Comment)
{
}

/// <summary>
///     This class was used for code generated and human edit.
/// </summary>
public sealed class TranslationItem
{
    /// <summary>
    ///     The translation id of this item.
    /// </summary>
    [XmlElement]
    public string Text { get; set; } = "";

    /// <summary>
    ///     The translation comment of this id.
    /// </summary>
    [XmlElement]
    public string Comment { get; set; } = string.Empty;

    /// <summary>
    ///     The translated of this id.
    /// </summary>
    [XmlElement]
    public string Translated { get; set; } = string.Empty;
}

[XmlRoot(nameof(TranslationItems), Namespace = Xml.Namespace)]
public class TranslationItems
{
    public const string TranslationsElementName = "Translations";

    public const string TranslationItemElementName = "Translation";

    [XmlArray(TranslationsElementName)]
    [XmlArrayItem(TranslationItemElementName)]
    public List<TranslationItem> Translations { get; set; } = [];
}

public sealed class TranslationProject
{
    public required LanguageID Identification { get; init; }

    public required FrozenDictionary<string, string> Items { get; init; }
}
