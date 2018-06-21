using System.Threading.Tasks;
using UnityEngine;

namespace UnityCommon
{
    /// <summary>
    /// Converts <see cref="byte[]"/> raw data of a .jpg or .png image to <see cref="Sprite"/>.
    /// </summary>
    public class JpgOrPngToSpriteConverter : IRawConverter<Sprite>
    {
        public RawDataRepresentation[] Representations { get { return new RawDataRepresentation[] {
            new RawDataRepresentation(".png", "image/png"),
            new RawDataRepresentation(".jpg", "image/jpeg")
        }; } }

        public Task<Sprite> ConvertAsync (byte[] obj)
        {
            var texture = new Texture2D(2, 2);
            texture.LoadImage(obj, true);
            var rect = new Rect(0, 0, texture.width, texture.height);
            var sprite = Sprite.Create(texture, rect, Vector2.one * .5f);
            return Task.FromResult(sprite);
        }

        public async Task<object> ConvertAsync (object obj) => await ConvertAsync(obj as byte[]);
    }
}
