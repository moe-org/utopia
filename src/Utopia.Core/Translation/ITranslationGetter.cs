namespace Utopia.Core.Translation;

/// <summary>
///     The interface allows you to get the translation.
///     And you should only get translation from this.
///     The message will be formatted by <see cref="Jeffijoe.MessageFormat.IMessageFormatter" />.
/// </summary>
public interface ITranslationGetter : IObserver<LanguageID>, IDisposable
{
    LanguageID CurrentLanguage { get; }

    string I18n(string text, string comment);

    string I18nf(string text, Dictionary<string, object?> args, string comment);
}
