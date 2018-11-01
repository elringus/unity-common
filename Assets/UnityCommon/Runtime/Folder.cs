using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityCommon
{
    /// <summary>
    /// Represents a directory in file systems.
    /// </summary>
    [System.Serializable]
    public class Folder : ScriptableObject
    {
        public string Path { get { return path; } private set { path = value; } }
        public string Name { get { return Path.Contains("/") ? Path.GetAfter("/") : Path; } }

        [SerializeField] string path = null;

        public new static Folder CreateInstance (string path)
        {
            var folder = CreateInstance<Folder>();
            folder.path = path;
            return folder;
        }
    }

    public static class FolderExtensions
    {
        public static IEnumerable<Folder> FindAllAtPath (this IEnumerable<Folder> folders, string path)
        {
            if (string.IsNullOrEmpty(path))
                return folders.Where(f => !f.Path.Contains("/") || string.IsNullOrEmpty(f.Path.GetBeforeLast("/")));
            return folders.Where(f => f.Path.GetBeforeLast("/").Equals(path) || f.Path.GetBeforeLast("/").Equals("/" + path));
        }
    }
}
