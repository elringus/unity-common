using UniRx.Async;

namespace UnityCommon
{
    /// <summary>
    /// Implentation is able to convert objects.
    /// </summary>
    public interface IConverter
    {
        object Convert (object obj, string name);

        UniTask<object> ConvertAsync (object obj, string name);
    }

    /// <summary>
    /// Implentation is able to convert <typeparamref name="TSource"/> to <typeparamref name="TResult"/>.
    /// </summary>
    public interface IConverter<TSource, TResult> : IConverter
    {
        TResult Convert (TSource obj, string name);

        UniTask<TResult> ConvertAsync (TSource obj, string name);
    }
}
