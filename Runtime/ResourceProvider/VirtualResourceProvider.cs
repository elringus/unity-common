using System;
using System.Collections.Generic;
using System.Linq;
using UniRx.Async;

namespace UnityCommon
{
    /// <summary>
    /// A mock <see cref="IResourceProvider"/> implementation allowing to add resources at runtime.
    /// </summary>
    public class VirtualResourceProvider : IResourceProvider
    {
        /// <summary>
        /// Whether <see cref="UnloadResource(string)"/> and similar methods should remove the added resources.
        /// </summary>
        public bool RemoveResourcesOnUnload { get; set; } = true;
        public bool IsLoading => false;
        public float LoadProgress => 1;
        public IReadOnlyCollection<Resource> LoadedResources => Resources?.Values;

        #pragma warning disable 0067
        public event Action<float> OnLoadProgress;
        public event Action<string> OnMessage;
        #pragma warning restore 0067

        protected readonly Dictionary<string, Resource> Resources;
        protected readonly HashSet<string> FolderPaths;

        public VirtualResourceProvider ()
        {
            Resources = new Dictionary<string, Resource>();
            FolderPaths = new HashSet<string>();
        }

        public bool SupportsType<T> () where T : UnityEngine.Object => true;

        public void AddResource<T> (string path, T obj) where T : UnityEngine.Object
        {
            Resources[path] = new Resource<T>(path, obj);
        }

        public void RemoveResource (string path)
        {
            Resources.Remove(path);
        }

        public void RemoveAllResources ()
        {
            Resources.Clear();
        }

        public void AddFolder (string folderPath)
        {
            FolderPaths.Add(folderPath);
        }

        public void RemoveFolder (string path)
        {
            FolderPaths.Remove(path);
        }

        public Resource<T> LoadResource<T> (string path) where T : UnityEngine.Object
        {
            return Resources.TryGetValue(path, out var resource) ? resource as Resource<T> : null;
        }

        public UniTask<Resource<T>> LoadResourceAsync<T> (string path) where T : UnityEngine.Object
        {
            var resource = LoadResource<T>(path);
            return UniTask.FromResult(resource);
        }

        public IReadOnlyCollection<Resource<T>> LoadResources<T> (string path) where T : UnityEngine.Object
        {
            return Resources.Where(kv => kv.Value?.Object.GetType() == typeof(T)).Select(kv => kv.Key).LocateResourcePathsAtFolder(path).Select(LoadResource<T>).ToArray();
        }

        public UniTask<IReadOnlyCollection<Resource<T>>> LoadResourcesAsync<T> (string path) where T : UnityEngine.Object
        {
            var resources = LoadResources<T>(path);
            return UniTask.FromResult(resources);
        }

        public IReadOnlyCollection<Folder> LocateFolders (string path)
        {
            return FolderPaths.LocateFolderPathsAtFolder(path).Select(p => new Folder(p)).ToArray();
        }

        public UniTask<IReadOnlyCollection<Folder>> LocateFoldersAsync (string path)
        {
            var folders = LocateFolders(path);
            return UniTask.FromResult(folders);
        }

        public IReadOnlyCollection<string> LocateResources<T> (string path) where T : UnityEngine.Object
        {
            return Resources.Where(kv => kv.Value?.Object.GetType() == typeof(T)).Select(kv => kv.Key).LocateResourcePathsAtFolder(path).ToArray();
        }

        public UniTask<IReadOnlyCollection<string>> LocateResourcesAsync<T> (string path) where T : UnityEngine.Object
        {
            var resources = LocateResources<T>(path);
            return UniTask.FromResult(resources);
        }

        public bool ResourceExists<T> (string path) where T : UnityEngine.Object
        {
            return Resources.ContainsKey(path) && Resources[path].Object.GetType() == typeof(T);
        }

        public UniTask<bool> ResourceExistsAsync<T> (string path) where T : UnityEngine.Object
        {
            var result = ResourceExists<T>(path);
            return UniTask.FromResult(result);
        }

        public bool ResourceLoaded (string path)
        {
            return ResourceExists<UnityEngine.Object>(path);
        }

        public bool ResourceLoading (string path)
        {
            return false;
        }

        public void UnloadResource (string path)
        {
            if (RemoveResourcesOnUnload)
                RemoveResource(path);
        }

        public UniTask UnloadResourceAsync (string path)
        {
            UnloadResource(path);
            return UniTask.CompletedTask;
        }

        public void UnloadResources ()
        {
            if (RemoveResourcesOnUnload)
                RemoveAllResources();
        }

        public UniTask UnloadResourcesAsync ()
        {
            UnloadResources();
            return UniTask.CompletedTask;
        }

        public Resource<T> GetLoadedResourceOrNull<T> (string path) where T : UnityEngine.Object
        {
            if (!ResourceLoaded(path)) return null;
            return LoadResource<T>(path);
        }
    }
}
