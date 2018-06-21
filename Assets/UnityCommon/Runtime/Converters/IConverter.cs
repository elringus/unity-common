using System.Threading.Tasks;

namespace UnityCommon
{
    /// <summary>
    /// Implentation is able to asynchronously convert objects.
    /// </summary>
    public interface IConverter
    {
        Task<object> ConvertAsync (object obj);
    }

    /// <summary>
    /// Implentation is able to asynchronously convert <see cref="TSource"/> to <see cref="TResult"/>.
    /// </summary>
    public interface IConverter<TSource, TResult> : IConverter
    {
        Task<TResult> ConvertAsync (TSource obj);
    }
}
