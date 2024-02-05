// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using Utopia.Core.Translation;

namespace Utopia.Server.Plugin;

internal class TranslationInformation
{
    public required ITranslationGetter Getter { private get; init; }

    public string xxxx_Description => Getter.I18n("", "");
}
