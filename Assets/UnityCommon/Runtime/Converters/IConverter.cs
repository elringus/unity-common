
/// <summary>
/// Implentation is able to convert objects.
/// </summary>
public interface IConverter
{
    object Convert (object obj);
}

/// <summary>
/// Implentation is able to convert <see cref="TSource"/> to <see cref="TResult"/>.
/// </summary>
public interface IConverter<TSource, TResult> : IConverter
{
    TResult Convert (TSource obj);
}
