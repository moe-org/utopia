#region

using Jeffijoe.MessageFormat;

#endregion

namespace Utopia.Core.Translation;

public class TranslationGetter : ITranslationGetter
{
    private readonly Lazy<MessageFormatter> _messageFormatter;
    protected bool _disposed;

    protected IDisposable subscriber;

    public TranslationGetter(ITranslationManager manager, IObservableExtension<LanguageID> id)
    {
        this.Manager = manager;

        this.CurrentLanguage = id.LastUpdateValue;

        this.subscriber = id.Subscribe(this);

        this._messageFormatter =
            new Lazy<MessageFormatter>(() => { return new MessageFormatter(true, this.CurrentLanguage.Location); },
                true);
    }

    public ITranslationManager Manager { get; init; }

    public LanguageID CurrentLanguage { get; private set; }

    public string I18n(string text, string comment)
    {
        if (this.Manager.TryGetTranslation(this.CurrentLanguage,
                text,
                out var msg
            ))
            return msg;

        return text;
    }

    public string I18nf(string text,
        Dictionary<string, object?> args,
        string comment)
    {
        if (this.Manager.TryGetTranslation(this.CurrentLanguage,
                text,
                out var msg
            ))
            return this._messageFormatter.Value.FormatMessage(msg, args);

        return this._messageFormatter.Value.FormatMessage(text, args);
    }

    public void OnCompleted()
    {
    }

    public void OnError(Exception error)
    {
    }

    public void OnNext(LanguageID value)
    {
        this.CurrentLanguage = value;
    }

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (this._disposed) return;

        if (disposing) this.subscriber.Dispose();

        this._disposed = true;
    }
}