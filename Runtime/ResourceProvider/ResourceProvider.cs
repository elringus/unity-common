using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityCommon
{
    /// <summary>
    /// A base <see cref="IResourceProvider"/> implementation.
    /// </summary>
    public abstract class ResourceProvider : IResourceProvider
    {
        public event Action<float> OnLoadProgress;
        public event Action<string> OnMessage;

        public bool IsLoading => LoadProgress < 1f;
        public float LoadProgress { get; private set; } = 1f;
        IEnumerable<Resource> IResourceProvider.LoadedResources => LoadedResources.Values;

        protected Dictionary<string, Resource> LoadedResources = new Dictionary<string, Resource>();
        protected Dictionary<string, List<Folder>> LocatedFolders = new Dictionary<string, List<Folder>>();
        protected Dictionary<string, ResourceRunner> LoadRunners = new Dictionary<string, ResourceRunner>();
        protected Dictionary<Tuple<string, Type>, ResourceRunner> LocateRunners = new Dictionary<Tuple<string, Type>, ResourceRunner>();

        public abstract bool SupportsType<T> () where T : UnityEngine.Object;

        public Resource<T> GetLoadedResourceOrNull<T> (string path) where T : UnityEngine.Object
        {
            if (!SupportsType<T>() || !ResourceLoaded(path)) return null;

            var loadedResourceType = LoadedResources[path].Object?.GetType();
            if (loadedResourceType != typeof(T))
            {
                Debug.LogError($"Failed to get a loaded resource with path `{path}`: the loaded resource is of type `{loadedResourceType.FullName}`, while the requested type is `{typeof(T).FullName}`.");
                return null;
            }
            else return LoadedResources[path] as Resource<T>;
        }

        public virtual async Task<Resource<T>> LoadResourceAsync<T> (string path) where T : UnityEngine.Object
        {
            if (!SupportsType<T>()) return null;

            if (ResourceLoading(path))
            {
                if (LoadRunners[path].ResourceType != typeof(T)) UnloadResource(path);
                else return await (LoadRunners[path] as LoadResourceRunner<T>);
            }

            if (ResourceLoaded(path))
            {
                if (LoadedResources[path].Object?.GetType() != typeof(T)) UnloadResource(path);
                else return LoadedResources[path] as Resource<T>;
            }

            var loadRunner = CreateLoadResourceRunner<T>(path);
            LoadRunners.Add(path, loadRunner);
            UpdateLoadProgress();

            RunResourceLoader(loadRunner);
            var resource = await loadRunner;

            HandleResourceLoaded(resource);
            return resource;
        }

        public virtual async Task<IEnumerable<Resource<T>>> LoadResourcesAsync<T> (string path) where T : UnityEngine.Object
        {
            if (!SupportsType<T>()) return null;

            var locatedResourcePaths = await LocateResourcesAsync<T>(path);
            return await Task.WhenAll(locatedResourcePaths.Select(p => LoadResourceAsync<T>(p)));
        }

        public virtual void UnloadResource (string path)
        {
            if (ResourceLoading(path))
                CancelResourceLoading(path);

            if (!ResourceLoaded(path)) return;

            var resource = LoadedResources[path];
            LoadedResources.Remove(path);

            DisposeResource(resource);

            LogMessage($"Resource '{path}' unloaded.");
        }

        public virtual void UnloadResources ()
        {
            var loadedPaths = LoadedResources.Values.Select(r => r.Path).ToList();
            foreach (var path in loadedPaths)
                UnloadResource(path);
        }

        public virtual bool ResourceLoaded (string path)
        {
            return LoadedResources.ContainsKey(path);
        }

        public virtual bool ResourceLoading (string path)
        {
            return LoadRunners.ContainsKey(path);
        }

        public virtual bool ResourceLocating<T> (string path)
        {
            return LocateRunners.ContainsKey(new Tuple<string, Type>(path, typeof(T)));
        }

        public virtual async Task<bool> ResourceExistsAsync<T> (string path) where T : UnityEngine.Object
        {
            if (!SupportsType<T>()) return false;
            if (ResourceLoaded<T>(path)) return true;
            var folderPath = path.Contains("/") ? path.GetBeforeLast("/") : string.Empty;
            var locatedResourcePaths = await LocateResourcesAsync<T>(folderPath);
            return locatedResourcePaths.Any(p => p.EqualsFast(path));
        }

        public virtual async Task<IEnumerable<string>> LocateResourcesAsync<T> (string path) where T : UnityEngine.Object
        {
            if (!SupportsType<T>()) return null;

            if (path is null) path = string.Empty;

            var locateKey = new Tuple<string, Type>(path, typeof(T));

            if (ResourceLocating<T>(path))
                return await (LocateRunners[locateKey] as LocateResourcesRunner<T>);

            var locateRunner = CreateLocateResourcesRunner<T>(path);
            LocateRunners.Add(locateKey, locateRunner);
            UpdateLoadProgress();

            RunResourcesLocator(locateRunner);

            var locatedResourcePaths = await locateRunner;
            HandleResourcesLocated<T>(locatedResourcePaths, path);
            return locatedResourcePaths;
        }

        public virtual async Task<IEnumerable<Folder>> LocateFoldersAsync (string path)
        {
            if (path is null) path = string.Empty;

            if (LocatedFolders.ContainsKey(path)) return LocatedFolders[path];

            var locateKey = new Tuple<string, Type>(path, typeof(Folder));

            if (ResourceLocating<Folder>(path))
                return await (LocateRunners[locateKey] as LocateFoldersRunner);

            var locateRunner = CreateLocateFoldersRunner(path);
            LocateRunners.Add(locateKey, locateRunner);
            UpdateLoadProgress();

            RunFoldersLocator(locateRunner);

            var locatedFolders  = await locateRunner;
            HandleFoldersLocated(locatedFolders, path);
            return locatedFolders;
        }

        public void LogMessage (string message) => OnMessage?.Invoke(message);

        protected abstract LoadResourceRunner<T> CreateLoadResourceRunner<T> (string path) where T : UnityEngine.Object;
        protected abstract LocateResourcesRunner<T> CreateLocateResourcesRunner<T> (string path) where T : UnityEngine.Object;
        protected abstract LocateFoldersRunner CreateLocateFoldersRunner (string path);
        protected abstract void DisposeResource (Resource resource);

        protected virtual void RunResourceLoader<T> (LoadResourceRunner<T> loader) where T : UnityEngine.Object => loader.RunAsync().WrapAsync();
        protected virtual void RunResourcesLocator<T> (LocateResourcesRunner<T> locator) where T : UnityEngine.Object => locator.RunAsync().WrapAsync();
        protected virtual void RunFoldersLocator (LocateFoldersRunner locator) => locator.RunAsync().WrapAsync();

        protected virtual bool ResourceLoaded<T> (string path) where T : UnityEngine.Object
        {
            return ResourceLoaded(path) && LoadedResources[path].Object.GetType() == typeof(T);
        }

        protected virtual void CancelResourceLoading (string path)
        {
            if (!ResourceLoading(path)) return;

            LoadRunners[path].Cancel();
            LoadRunners.Remove(path);

            UpdateLoadProgress();
        }

        protected virtual void CancelResourceLocating<T> (string path)
        {
            if (!ResourceLocating<T>(path)) return;

            var locateKey = new Tuple<string, Type>(path, typeof(T));

            LocateRunners[locateKey].Cancel();
            LocateRunners.Remove(locateKey);

            UpdateLoadProgress();
        }

        protected virtual void HandleResourceLoaded<T> (Resource<T> resource) where T : UnityEngine.Object
        {
            if (!resource.IsValid) Debug.LogError($"Resource '{resource.Path}' failed to load.");
            else LoadedResources[resource.Path] = resource;

            if (LoadRunners.ContainsKey(resource.Path))
                LoadRunners.Remove(resource.Path);

            UpdateLoadProgress();
        }

        protected virtual void HandleResourcesLocated<T> (IEnumerable<string> locatedResourcePaths, string path) where T : UnityEngine.Object
        {
            var locateKey = new Tuple<string, Type>(path, typeof(T));
            LocateRunners.Remove(locateKey);

            UpdateLoadProgress();
        }

        protected virtual void HandleFoldersLocated (IEnumerable<Folder> locatedFolders, string path)
        {
            var locateKey = new Tuple<string, Type>(path, typeof(Folder));
            LocateRunners.Remove(locateKey);

            LocatedFolders[path] = locatedFolders.ToList();

            UpdateLoadProgress();
        }

        protected virtual void UpdateLoadProgress ()
        {
            var prevProgress = LoadProgress;
            var runnersCount = LoadRunners.Count + LocateRunners.Count;
            if (runnersCount == 0) LoadProgress = 1f;
            else LoadProgress = Mathf.Min(1f / runnersCount, .999f);
            if (prevProgress != LoadProgress) OnLoadProgress?.Invoke(LoadProgress);
        }
    }
}
