// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Diagnostics;

namespace Utopia.Core.Translation;

/// <summary>
/// 默认翻译管理器，线程安全。
/// </summary>
public class TranslationManager : ITranslationManager
{
    private readonly List<ITranslationProvider> _providers = [];

    private readonly object _lock = new();

    public bool TryGetTranslation(LanguageID language, string item, [NotNullWhen(true)] out string? result)
    {
        Guard.IsNotNull(language);
        Guard.IsNotNull(item);

        lock (_lock)
        {
            foreach (var provider in _providers)
            {
                if (provider.TryGetItem(language, item, out result))
                {
                    return true;
                }
            }

            result = null;
            return false;
        }
    }

    public void AddTranslationProvider(ITranslationProvider provider)
    {
        lock (_lock)
        {
            if (_providers.Contains(provider))
            {
                return;
            }
            _providers.Add(provider);
        }
    }
}
