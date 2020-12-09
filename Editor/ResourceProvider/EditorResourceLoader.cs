using System;
using System.Collections.Generic;
using UniRx.Async;
using UnityEngine;

namespace UnityCommon
{
    public class EditorResourceLoader<TResource> : LoadResourceRunner<TResource> 
        where TResource : UnityEngine.Object
    {
        private readonly Dictionary<string, string> pathToGuidMap;
        private readonly Action<string> logAction;

        public EditorResourceLoader (IResourceProvider provider, string resourcePath,
            Dictionary<string, string> pathToGuidMap, Action<string> logAction) : base (provider, resourcePath)
        {
            this.pathToGuidMap = pathToGuidMap;
            this.logAction = logAction;
        }

        public override UniTask RunAsync ()
        {
            var startTime = Time.time;
            var obj = LoadEditorResource<TResource>(Path, pathToGuidMap);
            if (ObjectUtils.IsValid(obj))
                logAction?.Invoke($"Resource `{Path}` loaded over {Time.time - startTime:0.###} seconds.");
            var result = new Resource<TResource>(Path, obj);
            SetResult(result);
            return UniTask.CompletedTask;
        }

        public static T LoadEditorResource<T> (string path, Dictionary<string, string> pathToGuidMap) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(path) || !pathToGuidMap.TryGetValue(path, out var resourceGuid) || string.IsNullOrEmpty(resourceGuid))
                throw new Exception($"Resource `{path}` failed to load: resource path is invalid or wasn't mapped to an editor asset GUID.");

            var assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(resourceGuid);
            if (string.IsNullOrEmpty(assetPath))
                throw new Exception($"Resource `{path}` failed to load: AssetDatabase failed to find asset path by the mapped editor asset GUID ({resourceGuid}).");

            return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPath);
        }
    }
}
