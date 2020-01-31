using UniRx.Async;
using UnityEngine;

namespace UnityCommon
{
    /// <summary>
    /// Converts <see cref="T:byte[]"/> raw data of a .png image to <see cref="Texture2D"/>.
    /// </summary>
    public class PngToTextureConverter : IRawConverter<Texture2D>
    {
        public RawDataRepresentation[] Representations { get { return new RawDataRepresentation[] {
            new RawDataRepresentation(".png", "image/png")
        }; } }

        public Texture2D Convert (byte[] obj)
        {
            var texture = new Texture2D(2, 2);
            texture.LoadImage(obj, true);
            return texture;
        }

        public UniTask<Texture2D> ConvertAsync (byte[] obj)
        {
            var texture = Convert(obj);
            return UniTask.FromResult(texture);
        }

        public object Convert (object obj) => Convert(obj as byte[]);

        public async UniTask<object> ConvertAsync (object obj) => await ConvertAsync(obj as byte[]);
    }
}
