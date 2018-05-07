using System.Threading.Tasks;
using UnityEngine;

public class GFolderToFolderConverter : IRawConverter<Folder>
{
    public RawDataRepresentation[] Representations { get { return new RawDataRepresentation[] {
        new RawDataRepresentation(null, "application/vnd.google-apps.folder")
    }; } }

    public Task<Folder> ConvertAsync (byte[] obj)
    {
        Debug.LogError("Google Drive folders doesn't have binary content and are not downloadable.");
        return null;
    }

    public async Task<object> ConvertAsync (object obj) => await ConvertAsync(obj as byte[]);
}
