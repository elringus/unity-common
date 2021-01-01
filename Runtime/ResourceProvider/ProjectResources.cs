using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace UnityCommon
{
    public class ProjectResources : ScriptableObject
    {
        public IReadOnlyCollection<string> ResourcePaths => resourcePaths;

        [SerializeField] private List<string> resourcePaths = new List<string>();

        private void Awake ()
        {
            LocateAllResources();
        }

        public static ProjectResources Get ()
        {
            return Application.isEditor ? CreateInstance<ProjectResources>() : Resources.Load<ProjectResources>(nameof(ProjectResources));
        }

        public void LocateAllResources ()
        {
            if (!Application.isEditor) return;

            resourcePaths.Clear();
            var dataDir = new DirectoryInfo(Application.dataPath);
            var resourcesDirs = dataDir.GetDirectories("*Resources", SearchOption.AllDirectories)
                .Where(d => d.FullName.EndsWithFast($"{Path.DirectorySeparatorChar}Resources")).ToList();
            foreach (var dir in resourcesDirs)
                WalkResourcesDirectory(dir, resourcePaths);
        }

        private static void WalkResourcesDirectory (DirectoryInfo directory, List<string> outPaths)
        {
            var paths = directory.GetFiles().Where(IsNotMetaFile).Select(GetResourcePath);
            outPaths.AddRange(paths);

            var subDirs = directory.GetDirectories();
            foreach (var dirInfo in subDirs)
                WalkResourcesDirectory(dirInfo, outPaths);

            bool IsNotMetaFile (FileInfo info) => !info.FullName.EndsWithFast(".meta");
            
            string GetResourcePath (FileInfo info)
            {
                var path = info.FullName.Replace("\\", "/").GetAfterFirst("/Resources/");
                return path.Contains(".") ? path.GetBeforeLast(".") : path;
            }
        }
    }
}
