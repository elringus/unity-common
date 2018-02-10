using UnityEngine;

public class DirectoryToFolderConverter : IRawConverter<Folder>
{
    public RawDataRepresentation[] Representations { get { return null; } }

    public Folder Convert (byte[] obj)
    {
        Debug.LogError("Directory doesn't have binary content and can't be converted.");
        return null;
    }

    public object Convert (object obj)
    {
        return Convert(obj as byte[]);
    }
}
