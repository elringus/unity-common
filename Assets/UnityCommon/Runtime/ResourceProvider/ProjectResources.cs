using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityCommon
{
    public class ProjectResources : ScriptableObject
    {
        #pragma warning disable 0649
        [Serializable]
        private struct ProjectResource
        {
            public string Path, Type;
        }
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

        private void LocateAllResources ()
        {
            #if UNITY_EDITOR
            resourcePaths.Clear();
            foreach (var path in UnityEditor.AssetDatabase.GetAllAssetPaths())
            {
                if (!path.Contains("/Resources/")) continue;
                var type = UnityEditor.AssetDatabase.GetMainAssetTypeAtPath(path);
                if (type is null) continue;
                resourcePaths.Add(new ProjectResource { Path = GetPath(path), Type = type.AssemblyQualifiedName });
            }

            string GetPath (string path)
            {
                path = path.GetAfterFirst("/Resources/");
                return path.Contains(".") ? path.GetBeforeLast(".") : path;
            }
            #endif
        }
    }
}
