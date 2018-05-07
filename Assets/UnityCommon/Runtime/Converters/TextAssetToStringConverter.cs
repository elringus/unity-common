using System.Threading.Tasks;
using UnityEngine;

public class TextAssetToStringConverter : IConverter<TextAsset, string>
{
    public Task<string> ConvertAsync (TextAsset textAsset) => Task.FromResult(textAsset.text);

    public async Task<object> ConvertAsync (object obj) => await ConvertAsync(obj as TextAsset);
}
