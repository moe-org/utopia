// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace Utopia.Core.Translation;

public class TranslationProvider : ITranslationProvider
{
    private TranslationProject _Project { get; init; }

    public TranslationProvider(TranslationProject project)
    {
        ArgumentNullException.ThrowIfNull(project);
        _Project = project;
    }

    public bool Contain(LanguageID language, string item) => _Project.Identification.Equals(language) && _Project.Items.ContainsKey(item);

    public bool TryGetItem(LanguageID language, string item, [NotNullWhen(true)] out string? result)
    {
        if (!_Project.Identification.Equals(language))
        {
            result = null;
            return false;
        }

        return _Project.Items.TryGetValue(item, out result);
    }
}
