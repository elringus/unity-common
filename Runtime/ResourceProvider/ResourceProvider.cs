using System;
using System.Collections.Generic;
using System.Linq;
using UniRx.Async;
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
        IReadOnlyCollection<Resource> IResourceProvider.LoadedResources => LoadedResources.Values;

        protected readonly Dictionary<string, Resource> LoadedResources = new Dictionary<string, Resource>();
        protected readonly Dictionary<string, List<WeakReference>> Holders = new Dictionary<string, List<WeakReference>>();
        protected readonly Dictionary<string, List<Folder>> LocatedFolders = new Dictionary<string, List<Folder>>();
        protected readonly Dictionary<string, ResourceRunner> LoadRunners = new Dictionary<string, ResourceRunner>();
        protected readonly Dictionary<Tuple<string, Type>, ResourceRunner> LocateRunners = new Dictionary<Tuple<string, Type>, ResourceRunner>();
        protected readonly List<CachedResourceLocation> LocationsCache = new List<CachedResourceLocation>();

        public abstract bool SupportsType<T> () where T : UnityEngine.Object;

        public Resource<T> GetLoadedResourceOrNull<T> (string path) where T : UnityEngine.Object
        {
            if (!SupportsType<T>() || !ResourceLoaded(path)) return null;

            var loadedResource = LoadedResources[path];
            var loadedResourceType = loadedResource.Valid ? loadedResource.Object.GetType() : default;
            if (loadedResourceType != typeof(T))
                throw new Exception($"Failed to get a loaded resource with path `{path}`: The loaded resource is of type `{loadedResourceType?.FullName}`, while the requested type is `{typeof(T).FullName}`.");
            else return LoadedResources[path] as Resource<T>;
        }

        public virtual async UniTask<Resource<T>> LoadResourceAsync<T> (string path) where T : UnityEngine.Object
        {
            if (!SupportsType<T>()) return null;

            if (ResourceLoading(path))
            {
                if (LoadRunners[path].ResourceType != typeof(T)) UnloadResource(path);
                else return await (LoadResourceRunner<T>)LoadRunners[path];
            }

            if (ResourceLoaded(path))
            {
                var loadedResource = LoadedResources[path];
                if (!loadedResource.Valid || loadedResource.Object.GetType() != typeof(T)) UnloadResource(path);
                else return loadedResource as Resource<T>;
            }

            var loadRunner = CreateLoadResourceRunner<T>(path);
            LoadRunners.Add(path, loadRunner);
            UpdateLoadProgress();

            RunResourceLoader(loadRunner);
            var resource = await loadRunner;

            HandleResourceLoaded(resource);
            return resource;
        }

        public virtual async UniTask<IReadOnlyCollection<Resource<T>>> LoadResourcesAsync<T> (string path) where T : UnityEngine.Object
        {
            if (!SupportsType<T>()) return null;

            var locatedResourcePaths = await LocateResourcesAsync<T>(path);
            return await UniTask.WhenAll(locatedResourcePaths.Select(LoadResourceAsync<T>));
        }

        public virtual void UnloadResource (string path)
        {
            if (ResourceLoading(path))
                CancelResourceLoading(path);

            if (!ResourceLoaded(path)) return;

            var resource = LoadedResources[path];
            LoadedResources.Remove(path);

            // Make sure no other resources use the same object before disposing it.
            if (!LoadedResources.Any(r => r.Value.Object == resource.Object))
                DisposeResource(resource);

            LogMessage($"Resource `{path}` unloaded.");
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

        public virtual async UniTask<bool> ResourceExistsAsync<T> (string path) where T : UnityEngine.Object
        {
            if (LocationsCache.Count > 0) return IsLocationCached<T>(path);
            if (!SupportsType<T>()) return false;
            if (ResourceLoaded<T>(path)) return true;
            var folderPath = path.Contains("/") ? path.GetBeforeLast("/") : string.Empty;
            var locatedResourcePaths = await LocateResourcesAsync<T>(folderPath);
            return locatedResourcePaths.Any(p => p.EqualsFast(path));
        }

        public virtual async UniTask<IReadOnlyCollection<string>> LocateResourcesAsync<T> (string path) where T : UnityEngine.Object
        {
            if (!SupportsType<T>()) return null;
            if (path is null) path = string.Empty;

            if (LocationsCache.Count > 0)
                return LocateCached<T>(path);

            var locateKey = new Tuple<string, Type>(path, typeof(T));

            if (ResourceLocating<T>(path))
            {
                var locateTask = LocateRunners[locateKey] as LocateResourcesRunner<T>;
                if (locateTask is null) throw new Exception($"Failed to wait for `{path}` resource location runner.");
                await locateTask;
            }

            var locateRunner = CreateLocateResourcesRunner<T>(path);
            LocateRunners.Add(locateKey, locateRunner);
            UpdateLoadProgress();

            RunResourcesLocator(locateRunner);

            var locatedResourcePaths = await locateRunner;
            HandleResourcesLocated<T>(locatedResourcePaths, path);
            return locatedResourcePaths;
        }

        public virtual async UniTask<IReadOnlyCollection<Folder>> LocateFoldersAsync (string path)
        {
            if (path is null) path = string.Empty;

            if (LocatedFolders.ContainsKey(path)) return LocatedFolders[path];

            var locateKey = new Tuple<string, Type>(path, typeof(Folder));

            if (ResourceLocating<Folder>(path))
            {
                var locateTask = LocateRunners[locateKey] as LocateFoldersRunner;
                if (locateTask is null) throw new Exception($"Failed to wait for `{path}` folder location runner.");
                return await locateTask;
            }

            var locateRunner = CreateLocateFoldersRunner(path);
            LocateRunners.Add(locateKey, locateRunner);
            UpdateLoadProgress();

            RunFoldersLocator(locateRunner);

            var locatedFolders = await locateRunner;
            HandleFoldersLocated(locatedFolders, path);
            return locatedFolders;
        }

        public void LogMessage (string message) => OnMessage?.Invoke(message);

        public void Hold (string path, object holder)
        {
            GetHoldersFor(path).Add(new WeakReference(holder));
        }

        public void Release (string path, object holder, bool unload = true)
        {
            var holders = GetHoldersFor(path);
            Release(path, holders, holder, unload);
        }

        public void ReleaseAll (object holder, bool unload = true)
        {
            foreach (var kv in Holders)
                if (IsHeldBy(kv.Value, holder))
                    Release(kv.Key, kv.Value, holder, unload);
        }

        public bool IsHeldBy (string path, object holder) => IsHeldBy(GetHoldersFor(path), holder);

        public int CountHolders (string path) => CountHolders(GetHoldersFor(path));

        protected abstract LoadResourceRunner<T> CreateLoadResourceRunner<T> (string path) where T : UnityEngine.Object;
        protected abstract LocateResourcesRunner<T> CreateLocateResourcesRunner<T> (string path) where T : UnityEngine.Object;
        protected abstract LocateFoldersRunner CreateLocateFoldersRunner (string path);
        protected abstract void DisposeResource (Resource resource);

        protected virtual void RunResourceLoader<T> (LoadResourceRunner<T> loader) where T : UnityEngine.Object => loader.RunAsync().Forget();
        protected virtual void RunResourcesLocator<T> (LocateResourcesRunner<T> locator) where T : UnityEngine.Object => locator.RunAsync().Forget();
        protected virtual void RunFoldersLocator (LocateFoldersRunner locator) => locator.RunAsync().Forget();

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
            if (!resource.Valid) throw new Exception($"Resource `{resource.Path}` failed to load.");
            else LoadedResources[resource.Path] = resource;

            if (LoadRunners.ContainsKey(resource.Path))
                LoadRunners.Remove(resource.Path);

            UpdateLoadProgress();
        }

        protected virtual void HandleResourcesLocated<T> (IReadOnlyCollection<string> locatedResourcePaths, string path) where T : UnityEngine.Object
        {
            var locateKey = new Tuple<string, Type>(path, typeof(T));
            LocateRunners.Remove(locateKey);

            UpdateLoadProgress();
        }

        protected virtual void HandleFoldersLocated (IReadOnlyCollection<Folder> locatedFolders, string path)
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
            if (!Mathf.Approximately(prevProgress, LoadProgress)) OnLoadProgress?.Invoke(LoadProgress);
        }

        protected virtual bool AreTypesCompatible (Type sourceType, Type targetType) => sourceType == targetType;

        protected virtual bool IsLocationCached<T> (string path)
        {
            var targetType = typeof(T);
            return LocationsCache.Any(r => r.Path.EqualsFast(path) && AreTypesCompatible(r.Type, targetType));
        }

        protected virtual IReadOnlyCollection<string> LocateCached<T> (string path)
        {
            var targetType = typeof(T);
            return LocationsCache.Where(r => AreTypesCompatible(r.Type, targetType))
                .Select(r => r.Path).LocateResourcePathsAtFolder(path);
        }

        private List<WeakReference> GetHoldersFor (string path)
        {
            if (Holders.TryGetValue(path, out var holders)) return holders;
            holders = new List<WeakReference>();
            Holders[path] = holders;
            return holders;
        }

        private int CountHolders (List<WeakReference> holders)
        {
            return holders.Count(wr => wr.IsAlive);
        }

        private void Release (string path, List<WeakReference> holders, object holder, bool unload)
        {
            holders.RemoveAll(wr => !wr.IsAlive || wr.Target == holder);
            if (unload && CountHolders(holders) == 0)
                UnloadResource(path);
        }

        private bool IsHeldBy (List<WeakReference> holders, object holder)
        {
            return holders.Any(wr => wr.IsAlive && wr.Target == holder);
        }
    }
}
