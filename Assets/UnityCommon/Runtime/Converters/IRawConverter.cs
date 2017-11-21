
/// <summary>
/// Implementation is able to convert <see cref="byte[]"/> to <see cref="TResult"/>
/// and provide additional information about the raw data represenation of the object. 
/// </summary>
public interface IRawConverter<TResult> : IConverter<byte[], TResult> 
{
    string Extension { get; }
    string MimeType { get; }
}
