using UnityEngine;

/// <summary>
/// Converts <see cref="byte[]"/> raw data of a .png image to <see cref="Sprite"/>.
/// </summary>
public class PngToSpriteConverter : IRawConverter<Sprite>
{
    public RawDataRepresentation[] Representations { get { return new RawDataRepresentation[] {
        new RawDataRepresentation("png", "image/png")
    }; } }

    public Sprite Convert (byte[] obj)
    {
        var texture = new Texture2D(2, 2);
        texture.LoadImage(obj, true);
        var rect = new Rect(0, 0, texture.width, texture.height);
        return Sprite.Create(texture, rect, Vector2.one * .5f);
    }

    public object Convert (object obj)
    {
        return Convert(obj as byte[]);
    }
}
