using System.Threading.Tasks;
using UnityEngine;

namespace UnityCommon
{
    public class TextAssetToStringConverter : IConverter<TextAsset, string>
    {
        public string Convert (TextAsset textAsset) => textAsset.text;

        public Task<string> ConvertAsync (TextAsset textAsset) => Task.FromResult(textAsset.text);

        public object Convert (object obj) => Convert(obj as byte[]);

        public async Task<object> ConvertAsync (object obj) => await ConvertAsync(obj as TextAsset);
    }
}
