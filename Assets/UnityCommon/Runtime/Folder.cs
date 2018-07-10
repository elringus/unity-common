using UnityEngine;

namespace UnityCommon
{
    /// <summary>
    /// Represents a directory in file systems.
    /// </summary>
    [System.Serializable]
    public class Folder
    {
        public string Path { get { return path; } private set { path = value; } }
        public string Name { get { return Path.Contains("/") ? Path.GetAfter("/") : Path; } }

        [SerializeField] string path = null;

        public Folder (string path)
        {
            Path = path;
        }
    }
}
