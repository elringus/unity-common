using System;
using System.Collections.Generic;
using System.Linq;
using UniRx.Async;
using UnityEngine;

namespace UnityCommon
{
    /// <summary>
    /// Allows to to load and unload <see cref="Resource{TResource}"/> objects via a prioritized <see cref="IResourceProvider"/> list and local paths.
    /// </summary>
    public class ResourceLoader<TResource> : IResourceLoader<TResource> 
        where TResource : UnityEngine.Object
    {
        protected class TrackedResource
        {
            public readonly string FullPath;
            public readonly LinkedList<WeakReference> Holders = new LinkedList<WeakReference>();
            
            public TrackedResource (string fullPath)
            {
                FullPath = fullPath;
            }

            public void AddHolder (object holder) => Holders.AddLast(new WeakReference(holder));
            public void RemoveHolder (object holder) => Holders.RemoveAll(wr => !wr.IsAlive || wr.Target == holder);
            public bool IsHeldBy (object holder) => Holders.Any(wr => wr.Target == holder);
        }

        protected class LoadedResource : TrackedResource
        {
            public readonly Resource<TResource> Resource;
            public readonly IResourceProvider Provider;
            public bool Valid => Resource?.Valid ?? false;

            public LoadedResource (Resource<TResource> resource, IResourceProvider provider)
                : base(resource?.Path)
            {
                Resource = resource;
                Provider = provider;
            }
        }

        public event Action<string> OnResourceLoaded;
        public event Action<string> OnResourceUnloaded;
        
        /// <summary>
        /// Whether any of the providers used by this loader is currently loading anything.
        /// </summary>
        public virtual bool LoadingAny => Providers.AnyIsLoading();
        /// <summary>
        /// Prefix used by this provider to build full resource paths from provided local paths.
        /// </summary>
        public virtual string PathPrefix { get; }
        
        /// <summary>
        /// Prioritized providers list used by this loader.
        /// </summary>
        protected virtual List<IResourceProvider> Providers { get; }
        /// <summary>
        /// Resources loaded by this loader.
        /// </summary>
        protected readonly LinkedList<LoadedResource> LoadedResources = new LinkedList<LoadedResource>();
        
        private readonly LinkedList<TrackedResource> pendingHoldResources = new LinkedList<TrackedResource>();

        public ResourceLoader (IList<IResourceProvider> providersList, string resourcePathPrefix = null)
        { 
            Providers = new List<IResourceProvider>();
            Providers.AddRange(providersList);

            PathPrefix = resourcePathPrefix;
        }
        
        /// <summary>
        /// Given a local path to the resource, builds full path using predefined <see cref="PathPrefix"/>.
        /// </summary>
        public virtual string BuildFullPath (string localPath)
        {
            if (!string.IsNullOrWhiteSpace(PathPrefix))
            {
                if (!string.IsNullOrWhiteSpace(localPath)) return $"{PathPrefix}/{localPath}";
                else return PathPrefix;
            }
            else return localPath;
        }
        
        /// <summary>
        /// Given a full path to the resource, builds local path using predefined <see cref="PathPrefix"/>.
        /// </summary>
        public virtual string BuildLocalPath (string fullPath)
        {
            if (!string.IsNullOrWhiteSpace(PathPrefix))
            {
                var prefixAndSlash = $"{PathPrefix}/";
                if (!fullPath.Contains(prefixAndSlash))
                {
                    Debug.LogError($"Failed to build local path from `{fullPath}`: the provided path doesn't contain `{PathPrefix}` path prefix.");
                    return null;
                }
                return fullPath.GetAfterFirst(prefixAndSlash);
            }
            else return fullPath;
        }
        
        public virtual async void Hold (string path, object holder, bool fullPath = false)
        {
            if (!fullPath) path = BuildFullPath(path);
            
            var loadedResource = GetLoadedResource(path);
            if (loadedResource is null) // Attempt to load the resource in background.
            {
                // Check if the resource is already loading in background per another hold request.
                if (GetPendingHold(path) is TrackedResource loadingResource)
                {
                    if (loadingResource.IsHeldBy(holder)) return;
                    loadingResource.AddHolder(holder);
                    return;
                }
                
                var pendingResource = new TrackedResource(path);
                pendingHoldResources.AddLast(pendingResource);
                await LoadAsync(path, true);
                // Check if the resource has been requested to unload while it was loading.
                if (!pendingHoldResources.Contains(pendingResource)) { Unload(path, true); return; }
                loadedResource = GetLoadedResource(path);
                if (loadedResource is null) { Debug.LogError($"Failed to hold `{path}` resource: Resource is not available."); return; }
                // Transfer the holders added while the resource was loading.
                foreach (var ph in pendingResource.Holders)
                    loadedResource.Holders.AddLast(ph);
                pendingHoldResources.Remove(pendingResource);
            }

            if (loadedResource.IsHeldBy(holder)) return;
            loadedResource.AddHolder(holder);
        }

        public virtual void Release (string path, object holder, bool unload = true, bool fullPath = false)
        {
            if (!fullPath) path = BuildFullPath(path);

            var resource = GetLoadedResource(path) ?? GetPendingHold(path);
            if (resource is null) return;

            resource.RemoveHolder(holder);
            
            if (unload && resource.Holders.Count == 0)
                Unload(path, true);
        }

        public virtual bool IsHeldBy (string path, object holder, bool fullPath = false)
        {
            if (!fullPath) path = BuildFullPath(path);
            
            var resource = GetLoadedResource(path) ?? GetPendingHold(path);
            return resource?.IsHeldBy(holder) ?? false;
        }

        public virtual bool IsLoaded (string path, bool fullPath = false)
        {
            if (!fullPath) path = BuildFullPath(path);
            return Providers.ResourceLoaded(path);
        }

        public virtual Resource<TResource> GetLoadedOrNull (string path, bool fullPath = false)
        {
            if (!fullPath) path = BuildFullPath(path);
            return LoadedResources.FirstOrDefault(r => r.Valid && r.FullPath.EqualsFast(path))?.Resource;
        }

        public virtual async UniTask<Resource<TResource>> LoadAsync (string path, bool fullPath = false)
        {
            if (!fullPath) path = BuildFullPath(path);

            var (resource, provider) = await Providers.LoadResourceAsync<TResource>(path);
            if (resource != null && resource.Valid)
            {
                LoadedResources.AddLast(new LoadedResource(resource, provider));
                OnResourceLoaded?.Invoke(BuildLocalPath(path));
            }
            return resource;
        }

        public virtual async UniTask<IEnumerable<Resource<TResource>>> LoadAllAsync (string path = null, bool fullPath = false)
        {
            if (!fullPath) path = BuildFullPath(path);

            var loadResult = await Providers.LoadResourcesAsync<TResource>(path);
            foreach (var (resource, provider) in loadResult)
                if (resource != null && resource.Valid)
                {
                    LoadedResources.AddLast(new LoadedResource(resource, provider));
                    OnResourceLoaded?.Invoke(BuildLocalPath(path));
                }
            return loadResult.Select(t => t.Item1);
        }

        public virtual IEnumerable<Resource<TResource>> GetAllLoaded () => LoadedResources.Where(r => r.Valid).Select(r => r.Resource);

        public virtual async UniTask<IEnumerable<string>> LocateAsync (string path = null, bool fullPath = false)
        {
            if (!fullPath) path = BuildFullPath(path);
            var fullPaths = await Providers.LocateResourcesAsync<TResource>(path);
            return fullPaths.Select(t => BuildLocalPath(t.Item1));
        }

        public virtual async UniTask<bool> ExistsAsync (string path, bool fullPath = false)
        {
            if (!fullPath) path = BuildFullPath(path);
            var (exist, provider) = await Providers.ResourceExistsAsync<TResource>(path);
            return exist;
        }

        public virtual void Unload (string path, bool fullPath = false)
        {
            if (!fullPath) path = BuildFullPath(path);

            Providers.UnloadResource(path);
            LoadedResources.RemoveAll(r => !r.Valid || r.FullPath.EqualsFast(path));
            pendingHoldResources.RemoveAll(p => p.FullPath.EqualsFast(path));
            
            OnResourceUnloaded?.Invoke(BuildLocalPath(path));
        }

        public virtual void UnloadAll ()
        {
            foreach (var resource in LoadedResources)
            {
                resource.Provider.UnloadResource(resource.FullPath);
                OnResourceUnloaded?.Invoke(BuildLocalPath(resource.FullPath));
            }
            LoadedResources.Clear();
            pendingHoldResources.Clear();
        }
        
        protected virtual LoadedResource GetLoadedResource (string fullPath) => LoadedResources.FirstOrDefault(r => r.Valid && r.FullPath.EqualsFast(fullPath));
        
        private TrackedResource GetPendingHold (string fullPath) => pendingHoldResources.FirstOrDefault(r => r.FullPath.EqualsFast(fullPath));

        void IResourceLoader.Unload (string path) => Unload(path, false);
        void IResourceLoader.Hold (string path, object holder) => Hold(path, holder, false);
        void IResourceLoader.Release (string path, object holder, bool unload) => Release(path, holder, unload, false);
        bool IResourceLoader.IsHeldBy (string path, object holder) => IsHeldBy(path, holder, false);
        bool IResourceLoader.IsLoaded (string path) => IsLoaded(path, false);
        Resource<TResource> IResourceLoader<TResource>.GetLoadedOrNull (string path) => GetLoadedOrNull(path, false);
        Resource IResourceLoader.GetLoadedOrNull (string path) => GetLoadedOrNull(path, false);
        IEnumerable<Resource> IResourceLoader.GetAllLoaded () => GetAllLoaded();
        UniTask<Resource<TResource>> IResourceLoader<TResource>.LoadAsync (string path) => LoadAsync(path, false);
        UniTask<IEnumerable<Resource<TResource>>> IResourceLoader<TResource>.LoadAllAsync (string path) => LoadAllAsync(path, false);
        async UniTask<Resource> IResourceLoader.LoadAsync (string path) => await LoadAsync(path, false);
        async UniTask<IEnumerable<Resource>> IResourceLoader.LoadAllAsync (string path) => await LoadAllAsync(path, false);
        UniTask<IEnumerable<string>> IResourceLoader.LocateAsync (string path) => LocateAsync(path, false);
        UniTask<bool> IResourceLoader.ExistsAsync (string path) => ExistsAsync(path, false);
    }
}
