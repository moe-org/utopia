namespace Utopia.Core;

public interface IObservableExtension<out T> : IObservable<T>
{
    /// <summary>
    ///     上一次推送的值.即最新值.
    /// </summary>
    T LastUpdateValue { get; }
}