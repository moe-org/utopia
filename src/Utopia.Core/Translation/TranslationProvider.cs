#region

using System.Diagnostics.CodeAnalysis;

#endregion

namespace Utopia.Core.Translation;

public class TranslationProvider : ITranslationProvider
{
    public TranslationProvider(TranslationProject project)
    {
        ArgumentNullException.ThrowIfNull(project);
        this._Project = project;
    }

    private TranslationProject _Project { get; }

    public bool Contain(LanguageID language, string item)
    {
        return this._Project.Identification.Equals(language) && this._Project.Items.ContainsKey(item);
    }

    public bool TryGetItem(LanguageID language, string item, [NotNullWhen(true)] out string? result)
    {
        if (!this._Project.Identification.Equals(language))
        {
            result = null;
            return false;
        }

        return this._Project.Items.TryGetValue(item, out result);
    }
}
