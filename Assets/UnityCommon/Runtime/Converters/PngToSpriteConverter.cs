using UnityEngine;

/// <summary>
/// Converts <see cref="byte[]"/> to <see cref="Sprite"/>.
/// </summary>
public class PngToSpriteConverter : IRawConverter<Sprite>
{
    public string Extension { get { return "png"; } }
    public string MimeType { get { return "image/png"; } }

    public Sprite Convert (byte[] obj)
    {
        var texture = new Texture2D(2, 2);
        texture.LoadImage(obj);
        var rect = new Rect(0, 0, texture.width, texture.height);
        return Sprite.Create(texture, rect, Vector2.one * .5f);
    }

    public object Convert (object obj)
    {
        return Convert(obj as byte[]);
    }
}
