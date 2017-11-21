
/// <summary>
/// Implentation is able to convert <see cref="TSource"/> to <see cref="TResult"/>.
/// </summary>
public interface IConverter<TSource, TResult>
{
    TResult Convert (TSource obj);
}
