using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityCommon
{
    public class EditorResourceLoader<TResource> : LoadResourceRunner<TResource> where TResource : UnityEngine.Object
    {
        private Dictionary<string, string> pathToGuidMap;
        private Action<string> logAction;

        public EditorResourceLoader (Resource<TResource> resource, Dictionary<string, string> pathToGuidMap, Action<string> logAction)
        {
            Resource = resource;
            this.pathToGuidMap = pathToGuidMap;
            this.logAction = logAction;
        }

        public override async Task Run ()
        {
            await base.Run();

            var startTime = Time.time;

            Resource.Object = LoadEditorResource<TResource>(Resource?.Path, pathToGuidMap);

            if (Resource.IsValid)
                logAction?.Invoke($"Resource '{Resource.Path}' loaded over {Time.time - startTime:0.###} seconds.");

            HandleOnCompleted();
        }

        public static T LoadEditorResource<T> (string path, Dictionary<string, string> pathToGuidMap) where T : UnityEngine.Object
        {
            #if UNITY_EDITOR
            if (string.IsNullOrEmpty(path) || !pathToGuidMap.TryGetValue(path, out var resourceGuid) || string.IsNullOrEmpty(resourceGuid))
            {
                Debug.LogError($"Resource '{path}' failed to load: resource path is invalid or wasn't mapped to an editor asset GUID.");
                return null;
            }

            var assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(resourceGuid);
            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogError($"Resource '{path}' failed to load: AssetDatabase failed to find asset path by the mapped editor asset GUID ({resourceGuid}).");
                return null;
            }

            return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPath);
            #else 
            Debug.LogError($"Editor resource provider can't be used outside of the Unity editor.");
            return null;
            #endif
        }
    }
}
