using System.Text;
using UniRx.Async;
using UnityEngine;

namespace UnityCommon
{
    public class TxtToTextAssetConverter : IRawConverter<TextAsset>
    {
        public RawDataRepresentation[] Representations { get { return new RawDataRepresentation[] {
            new RawDataRepresentation(".txt", "text/plain")
        }; } }

        public TextAsset Convert (byte[] obj) => new TextAsset(Encoding.UTF8.GetString(obj));

        public UniTask<TextAsset> ConvertAsync (byte[] obj) => UniTask.FromResult(new TextAsset(Encoding.UTF8.GetString(obj)));

        public object Convert (object obj) => Convert(obj as byte[]);

        public async UniTask<object> ConvertAsync (object obj) => await ConvertAsync(obj as byte[]);
    }
}
