using System.Threading.Tasks;
using UnityEngine;

public class DirectoryToFolderConverter : IRawConverter<Folder>
{
    public RawDataRepresentation[] Representations { get { return null; } }

    public Task<Folder> ConvertAsync (byte[] obj)
    {
        Debug.LogError("Directory doesn't have binary content and can't be converted.");
        return Task.FromResult(default(Folder));
    }

    public async Task<object> ConvertAsync (object obj) => await ConvertAsync(obj as byte[]);
}
