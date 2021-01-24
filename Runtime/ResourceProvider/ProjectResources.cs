using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace UnityCommon
{
    public class ProjectResources : ScriptableObject
    {
        [Serializable]
        private struct ProjectResource { public string Path, Type; }
        
        public IReadOnlyDictionary<string, Type> Resources { get; private set; }

        [SerializeField] private List<ProjectResource> resourcePaths = new List<ProjectResource>();

        private void Awake ()
        {
            LocateAllResources();
            Resources = resourcePaths.ToDictionary(r => r.Path, r => Type.GetType(r.Type));
        }

        public static ProjectResources Get ()
        {
            return Application.isEditor ? CreateInstance<ProjectResources>() 
                : UnityEngine.Resources.Load<ProjectResources>(nameof(ProjectResources));
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

        private static void WalkResourcesDirectory (DirectoryInfo directory, List<ProjectResource> outPaths)
        {
            var paths = directory.GetFiles().Where(IsNotMetaFile).Select(GetAssetPath);
            foreach (var path in paths)
                AddPathUsingEditorAPI(path);

            var subDirs = directory.GetDirectories();
            foreach (var dirInfo in subDirs)
                WalkResourcesDirectory(dirInfo, outPaths);

            bool IsNotMetaFile (FileInfo info) => !info.FullName.EndsWithFast(".meta");
            
            string GetAssetPath (FileInfo info) => PathUtils.AbsoluteToAssetPath(info.FullName);
            
            string ToResourcePath (string assetPath)
            {
                assetPath = assetPath.GetAfterFirst("/Resources/");
                return assetPath.Contains(".") ? assetPath.GetBeforeLast(".") : assetPath;
            }

            void AddPathUsingEditorAPI (string assetPath)
            {
                #if UNITY_EDITOR
                var type = UnityEditor.AssetDatabase.GetMainAssetTypeAtPath(assetPath);
                if (type is null) throw new Exception($"Failed to get type of `{assetPath}` asset.");
                outPaths.Add(new ProjectResource { Path = ToResourcePath(assetPath), Type = type.AssemblyQualifiedName });
                #else 
                throw new Exception("Project resources can't be collected outside of editor.");
                #endif
            }
        }
    }
}
