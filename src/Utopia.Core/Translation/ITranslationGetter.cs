// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utopia.Core.Translation;

/// <summary>
/// The interface allows you to get the translation.
/// And you should only get translation from this.
/// The message will be formatted by <see cref="Jeffijoe.MessageFormat.IMessageFormatter"/>.
/// </summary>
public interface ITranslationGetter : IObserver<LanguageID>,IDisposable
{
    LanguageID CurrentLanguage { get; }

    string I18n(string text,string comment);

    string I18nf(string text, Dictionary<string,object?> args, string comment);
}
