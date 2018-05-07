using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Converts <see cref="byte[]"/> raw data of a .png image to <see cref="Texture2D"/>.
/// </summary>
public class PngToTextureConverter : IRawConverter<Texture2D>
{
    public RawDataRepresentation[] Representations { get { return new RawDataRepresentation[] {
        new RawDataRepresentation("png", "image/png")
    }; } }

    public Task<Texture2D> ConvertAsync (byte[] obj)
    {
        var texture = new Texture2D(2, 2);
        texture.LoadImage(obj, true);
        return Task.FromResult(texture);
    }

    public async Task<object> ConvertAsync (object obj) => await ConvertAsync(obj as byte[]);
}
