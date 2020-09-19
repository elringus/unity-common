using System;
using System.Collections.Generic;
using System.Linq;
using UniRx.Async;
using UnityEngine;

namespace UnityCommon
{
    /// <summary>
    /// Allows to load and unload <see cref="Resource{TResource}"/> objects via a prioritized <see cref="ProvisionSource"/> list.
    /// </summary>
    public class ResourceLoader<TResource> : IResourceLoader<TResource> 
        where TResource : UnityEngine.Object
    {
        protected class TrackedResource
        {
            public readonly string LocalPath;
            public readonly LinkedList<WeakReference> Holders = new LinkedList<WeakReference>();
            
            public TrackedResource (string localPath)
            {
                LocalPath = localPath;
            }

            public void AddHolder (object holder) => Holders.AddLast(new WeakReference(holder));
            public void RemoveHolder (object holder) => Holders.RemoveAll(wr => !wr.IsAlive || wr.Target == holder);
            public bool IsHeldBy (object holder) => Holders.Any(wr => wr.Target == holder);
        }

        protected class LoadedResource : TrackedResource
        {
            public readonly Resource<TResource> Resource;
            public readonly ProvisionSource ProvisionSource;
            public readonly string FullPath;
            public bool Valid => Resource.Valid;

            public LoadedResource (Resource<TResource> resource, ProvisionSource provisionSource)
                : base(ProvisionSource.BuildLocalPath(provisionSource.PathPrefix, resource.Path))
            {
                Resource = resource;
                ProvisionSource = provisionSource;
                FullPath = resource.Path;
            }
        }

        public event Action<string> OnResourceLoaded;
        public event Action<string> OnResourceUnloaded;
        
        /// <summary>
        /// Whether any of the providers used by this loader is currently loading anything.
        /// </summary>
        public virtual bool LoadingAny => ProvisionSources.Any(s => s.Provider.IsLoading);

        /// <summary>
        /// Prioritized provision sources list used by the loader.
        /// </summary>
        protected readonly List<ProvisionSource> ProvisionSources = new List<ProvisionSource>();
        /// <summary>
        /// Resources loaded by the loader.
        /// </summary>
        protected readonly LinkedList<LoadedResource> LoadedResources = new LinkedList<LoadedResource>();

        private readonly LinkedList<TrackedResource> pendingHoldResources = new LinkedList<TrackedResource>();

        public ResourceLoader (IList<ProvisionSource> provisionSources)
        { 
            ProvisionSources.AddRange(provisionSources);
        }
        
        public ResourceLoader (IList<IResourceProvider> providersList, string pathPrefix = null)
        { 
            foreach (var provider in providersList)
                ProvisionSources.Add(new ProvisionSource(provider, pathPrefix));
        }

        public virtual async void Hold (string path, object holder)
        {
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
                await LoadAsync(path);
                // Check if the resource has been requested to unload while it was loading.
                if (!pendingHoldResources.Contains(pendingResource)) { Unload(path); return; }
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

        public virtual void Release (string path, object holder, bool unload = true)
        {
            var resource = GetLoadedResource(path) ?? GetPendingHold(path);
            if (resource is null) return;

            resource.RemoveHolder(holder);
            
            if (unload && resource.Holders.Count == 0)
                Unload(path);
        }

        public virtual bool IsHeldBy (string path, object holder)
        {
            var resource = GetLoadedResource(path) ?? GetPendingHold(path);
            return resource?.IsHeldBy(holder) ?? false;
        }

        public virtual bool IsLoaded (string path)
        {
            return LoadedResources.Any(r => r.Valid && r.LocalPath.EqualsFast(path));
        }

        public virtual Resource<TResource> GetLoadedOrNull (string path)
        {
            return GetLoadedResource(path)?.Resource;
        }

        public virtual async UniTask<Resource<TResource>> LoadAsync (string path)
        {
            if (IsLoaded(path))
                return GetLoadedOrNull(path);

            foreach (var source in ProvisionSources)
            {
                var fullPath = source.BuildFullPath(path);
                if (!await source.Provider.ResourceExistsAsync<TResource>(fullPath)) continue;
                
                var resource = await source.Provider.LoadResourceAsync<TResource>(fullPath);
                LoadedResources.AddLast(new LoadedResource(resource, source));
                OnResourceLoaded?.Invoke(path);
                return resource;
            }
            
            return Resource<TResource>.Invalid;
        }

        public virtual async UniTask<IEnumerable<Resource<TResource>>> LoadAllAsync (string path = null)
        {
            var result = new List<Resource<TResource>>();
            
            foreach (var source in ProvisionSources)
            {
                var fullPath = source.BuildFullPath(path);
                var locatedResourcePaths = await source.Provider.LocateResourcesAsync<TResource>(fullPath);
                foreach (var locatedResourcePath in locatedResourcePaths)
                {
                    if (result.Any(r => r.Path.EqualsFast(locatedResourcePath))) continue;

                    var localPath = source.BuildLocalPath(locatedResourcePath);
                    
                    if (IsLoaded(localPath))
                    {
                        result.Add(GetLoadedOrNull(localPath));
                        continue;
                    }
                    
                    var resource = await source.Provider.LoadResourceAsync<TResource>(locatedResourcePath);
                    LoadedResources.AddLast(new LoadedResource(resource, source));
                    OnResourceLoaded?.Invoke(localPath);
                    result.Add(resource);
                }
            }

            return result;
        }

        public virtual IEnumerable<Resource<TResource>> GetAllLoaded ()
        {
            return LoadedResources.Where(r => r.Valid).Select(r => r.Resource);
        } 

        public virtual async UniTask<IEnumerable<string>> LocateAsync (string path = null)
        {
            var result = new List<string>();
            
            foreach (var source in ProvisionSources)
            {
                var fullPath = source.BuildFullPath(path);
                var locatedResourcePaths = await source.Provider.LocateResourcesAsync<TResource>(fullPath);
                foreach (var locatedResourcePath in locatedResourcePaths)
                {
                    var localPath = source.BuildLocalPath(locatedResourcePath);
                    if (!result.Any(p => p.EqualsFast(localPath)))
                        result.Add(localPath);
                }
            }
            
            return result;
        }

        public virtual async UniTask<bool> ExistsAsync (string path)
        {
            if (IsLoaded(path)) 
                return true;

            foreach (var source in ProvisionSources)
            {
                var fullPath = source.BuildFullPath(path);
                if (await source.Provider.ResourceExistsAsync<TResource>(fullPath)) 
                    return true;
            }

            return false;
        }

        public virtual void Unload (string path)
        {
            var resource = GetLoadedResource(path);
            resource?.ProvisionSource.Provider.UnloadResource(resource.FullPath);

            LoadedResources.RemoveAll(r => !r.Valid || r.LocalPath.EqualsFast(path));
            pendingHoldResources.RemoveAll(p => p.LocalPath.EqualsFast(path));
            
            OnResourceUnloaded?.Invoke(path);
        }

        public virtual void UnloadAll ()
        {
            foreach (var resource in LoadedResources)
            {
                resource.ProvisionSource.Provider.UnloadResource(resource.FullPath);
                OnResourceUnloaded?.Invoke(resource.LocalPath);
            }
            LoadedResources.Clear();
            pendingHoldResources.Clear();
        }
        
        protected virtual LoadedResource GetLoadedResource (string localPath) => LoadedResources.FirstOrDefault(r => r.Valid && r.LocalPath.EqualsFast(localPath));
        
        private TrackedResource GetPendingHold (string localPath) => pendingHoldResources.FirstOrDefault(r => r.LocalPath.EqualsFast(localPath));
        
        Resource IResourceLoader.GetLoadedOrNull (string path) => GetLoadedOrNull(path);
        IEnumerable<Resource> IResourceLoader.GetAllLoaded () => GetAllLoaded();
        async UniTask<Resource> IResourceLoader.LoadAsync (string path) => await LoadAsync(path);
        async UniTask<IEnumerable<Resource>> IResourceLoader.LoadAllAsync (string path) => await LoadAllAsync(path);
    }
}
