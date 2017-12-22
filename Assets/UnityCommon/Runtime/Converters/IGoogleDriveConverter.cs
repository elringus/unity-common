
/// <summary>
/// Implementation is able to convert exported google drive files to <see cref="TResult"/>.
/// </summary>
public interface IGoogleDriveConverter<TResult> : IRawConverter<TResult>
{
    string ExportMimeType { get; }
}
