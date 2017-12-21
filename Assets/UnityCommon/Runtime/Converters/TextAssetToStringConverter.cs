using UnityEngine;

public class TextAssetToStringConverter : IConverter<TextAsset, string>
{
    public string Convert (TextAsset textAsset)
    {
        return textAsset.text;
    }

    public object Convert (object obj)
    {
        return Convert(obj as TextAsset);
    }
}
