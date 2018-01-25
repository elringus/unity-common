
/// <summary>
/// Represents a directory in file systems.
/// </summary>
public class Folder 
{
    public string Name { get; private set; }

    public Folder (string name)
    {
        Name = name;
    }

    public static string ExtractNameFromPath (string path)
    {
        return path.Contains("/") ? path.GetAfter("/") : path;
    }
}
