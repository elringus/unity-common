using System.Text;
using UniRx.Async;
using UnityEngine;

namespace UnityCommon
{
    public class GDocToTextAssetConverter : IGoogleDriveConverter<TextAsset>
    {
        public RawDataRepresentation[] Representations { get; } = {
            new RawDataRepresentation(null, "application/vnd.google-apps.document")
        };

        public string ExportMimeType => "text/plain";

        public TextAsset Convert (byte[] obj, string name)
        {
            var textAsset = new TextAsset(Encoding.UTF8.GetString(obj));
            textAsset.name = name;
            return textAsset;
        }

        public UniTask<TextAsset> ConvertAsync (byte[] obj, string name) => UniTask.FromResult(Convert(obj, name));

        public object Convert (object obj, string name) => Convert(obj as byte[], name);

        public async UniTask<object> ConvertAsync (object obj, string name) => await ConvertAsync(obj as byte[], name);
    }
}
