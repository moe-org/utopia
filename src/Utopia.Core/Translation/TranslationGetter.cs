// This file is a part of the project Utopia(Or is a part of its subproject).
// Copyright 2020-2023 mingmoe(http://kawayi.moe)
// The file was licensed under the AGPL 3.0-or-later license

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac.Core;
using Jeffijoe.MessageFormat;
using SharpCompress;

namespace Utopia.Core.Translation;

public class TranslationGetter : ITranslationGetter
{
    protected bool _disposed = false;

    protected IDisposable subscriber;

    public ITranslationManager Manager { get; init; }

    public LanguageID CurrentLanguage { get; private set; }

    private readonly System.Lazy<MessageFormatter> _messageFormatter;

    public TranslationGetter(ITranslationManager manager, IObservableExtension<LanguageID> id)
    {
        Manager = manager;

        CurrentLanguage = id.LastUpdateValue;

        subscriber = id.Subscribe(this);

        _messageFormatter = new(() =>
        {
            return new(true, CurrentLanguage.Location);
        }, true);
    }

    public string I18n(string text, string comment)
    {
        if (Manager.TryGetTranslation(CurrentLanguage,
            text,
            out var msg
            ))
        {
            return msg;
        }

        return text;
    }

    public string I18nf(string text,
        Dictionary<string,object?> args,
        string comment)
    {
        if(Manager.TryGetTranslation(CurrentLanguage,
            text,
            out string? msg
            ))
        {
            return _messageFormatter.Value.FormatMessage(msg, args);
        }

        return _messageFormatter.Value.FormatMessage(text, args);
    }

    public void OnCompleted()
    {

    }

    public void OnError(Exception error)
    {

    }

    public void OnNext(LanguageID value)
    {
        CurrentLanguage = value;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            subscriber.Dispose();
        }

        _disposed = true;
    }
}
