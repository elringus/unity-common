using UniRx.Async;

namespace UnityCommon
{
    /// <summary>
    /// Implentation is able to convert objects.
    /// </summary>
    public interface IConverter
    {
        object Convert (object obj);

        UniTask<object> ConvertAsync (object obj);
    }

    /// <summary>
    /// Implentation is able to convert <typeparamref name="TSource"/> to <typeparamref name="TResult"/>.
    /// </summary>
    public interface IConverter<TSource, TResult> : IConverter
    {
        TResult Convert (TSource obj);

        UniTask<TResult> ConvertAsync (TSource obj);
    }
}
