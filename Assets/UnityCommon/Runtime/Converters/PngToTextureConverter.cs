using UnityEngine;

/// <summary>
/// Converts <see cref="byte[]"/> raw data of a .png image to <see cref="Texture2D"/>.
/// </summary>
public class PngToTextureConverter : IRawConverter<Texture2D>
{
    public string Extension { get { return "png"; } }
    public string MimeType { get { return "image/png"; } }

    public Texture2D Convert (byte[] obj)
    {
        var texture = new Texture2D(2, 2);
        texture.LoadImage(obj);
        return texture;
    }

    public object Convert (object obj)
    {
        return Convert(obj as byte[]);
    }
}
