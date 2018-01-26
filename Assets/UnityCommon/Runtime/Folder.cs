using UnityEngine;

/// <summary>
/// Represents a directory in file systems.
/// </summary>
[System.Serializable]
public class Folder 
{
    public string Path { get { return _path; } private set { _path = value; } }
    public string Name { get { return Path.Contains("/") ? Path.GetAfter("/") : Path; } }

    [SerializeField] string _path = null;

    public Folder (string path)
    {
        Path = path;
    }
}
