using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace UnityCommon
{
    public class ProjectResources : ScriptableObject
    {
        #pragma warning disable 0649
        [Serializable]
        private struct ProjectResource { public string Path, Type; }
        #pragma warning restore 0649

        public const string ResourcePath = "UnityCommon/ProjectResources";
        
        [SerializeField] private List<ProjectResource> resourcePaths = new List<ProjectResource>();

        public static ProjectResources Get ()
        {
            var asset = Application.isEditor
                ? CreateInstance<ProjectResources>()
                : Resources.Load<ProjectResources>(ResourcePath);
            if (Application.isEditor) asset.LocateAllResources();
            return asset;
        }

        public IReadOnlyDictionary<string, Type> GetAllResources (string prefixFilter = null)
        {
            var resources = new Dictionary<string, Type>();
            foreach (var resource in resourcePaths)
                AddResource(resource);
            return resources;

            void AddResource (ProjectResource resource)
            {
                if (prefixFilter != null && !resource.Path.StartsWithFast(prefixFilter)) return;
                var type = Type.GetType(resource.Type, false);
                if (type is null) return;
                var path = prefixFilter != null
                    ? resource.Path.GetAfterFirst(prefixFilter)
                    : resource.Path;
                resources[path] = type;
            }
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

            void AddPathUsingEditorAPI (string assetPath)
            {
                #if UNITY_EDITOR
                var type = UnityEditor.AssetDatabase.GetMainAssetTypeAtPath(assetPath);
                if (type is null) throw new Exception($"Failed to get type of `{assetPath}` asset.");
                outPaths.Add(new ProjectResource { Path = GetResourcePath(), Type = type.AssemblyQualifiedName });
                string GetResourcePath ()
                {
                    assetPath = assetPath.GetAfterFirst("/Resources/");
                    return assetPath.Contains(".") ? assetPath.GetBeforeLast(".") : assetPath;
                }
                #endif
            }
        }

        private void LocateAllResources ()
        {
            resourcePaths.Clear();
            var dataDir = new DirectoryInfo(Application.dataPath);
            var resourcesDirs = dataDir.GetDirectories("*Resources", SearchOption.AllDirectories)
                .Where(d => d.FullName.EndsWithFast($"{Path.DirectorySeparatorChar}Resources")).ToList();
            foreach (var dir in resourcesDirs)
                WalkResourcesDirectory(dir, resourcePaths);
        }
    }
}
