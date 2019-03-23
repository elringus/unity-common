using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityCommon
{
    /// <summary>
    /// Provides resources stored anywhere inside the project's `Assets` folder using provided path to GUID map; works only in the editor.
    /// </summary>
    public class EditorResourceProvider : ResourceProvider
    {
        private Dictionary<string, string> pathToGuidMap = new Dictionary<string, string>();

        public void AddResourceGuid (string path, string guid)
        {
            pathToGuidMap[path] = guid;
        }

        public void RemoveResourceGuid (string path)
        {
            pathToGuidMap.Remove(path);
        }

        public override bool SupportsType<T> () => true;

        protected override LoadResourceRunner<T> CreateLoadResourceRunner<T> (Resource<T> resource)
        {
            return new EditorResourceLoader<T>(resource, pathToGuidMap, LogMessage);
        }

        protected override LocateResourcesRunner<T> CreateLocateResourcesRunner<T> (string path)
        {
            return new EditorResourceLocator<T>(path, pathToGuidMap.Keys);
        }

        protected override LocateFoldersRunner CreateLocateFoldersRunner (string path)
        {
            return new EditorFolderLocator(path, pathToGuidMap.Keys);
        }

        protected override Resource<T> LoadResourceBlocking<T> (string path)
        {
            var obj = EditorResourceLoader<T>.LoadEditorResource<T>(path, pathToGuidMap);
            return ObjectUtils.IsValid(obj) ? new Resource<T>(path, obj) : null;
        }

        protected override IEnumerable<Resource<T>> LocateResourcesBlocking<T> (string path)
        {
            return EditorResourceLocator<T>.LocateProjectResources(path, pathToGuidMap.Keys);
        }

        protected override IEnumerable<Folder> LocateFoldersBlocking (string path)
        {
            return EditorFolderLocator.LocateEditorFolders(path, pathToGuidMap.Keys);
        }

        protected override void UnloadResourceBlocking (Resource resource)
        {
            if (!resource.IsValid) return;
            #if UNITY_EDITOR
            if (UnityEditor.AssetDatabase.Contains(resource.Object)) Resources.UnloadAsset(resource.Object);
            else if (!Application.isPlaying) Object.DestroyImmediate(resource.Object);
            else Object.Destroy(resource.Object);
            #endif
        }

        protected override Task UnloadResourceAsync (Resource resource)
        {
            UnloadResourceBlocking(resource);
            return Task.CompletedTask;
        }
    }
}
