using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityCommon
{
    /// <summary>
    /// Allows to load and unload <see cref="Resource{TResource}"/> objects via a prioritized <see cref="ProvisionSource"/> list.
    /// </summary>
    public class ResourceLoader<TResource> : IResourceLoader<TResource>
        where TResource : Object
    {
        protected class LoadedResource
        {
            public readonly Resource<TResource> Resource;
            public readonly ProvisionSource ProvisionSource;
            public readonly string LocalPath;
            public Object Object => Resource.Object;
            public string FullPath => Resource.Path;
            public bool Valid => Resource.Valid;
            public int HoldersCount => holders.Count;

            private readonly HashSet<object> holders = new HashSet<object>();

            public LoadedResource (Resource<TResource> resource, ProvisionSource provisionSource)
            {
                Resource = resource;
                ProvisionSource = provisionSource;
                LocalPath = provisionSource.BuildLocalPath(resource.Path);
            }

            public void AddHolder (object holder) => holders.Add(holder);
            public void RemoveHolder (object holder) => holders.Remove(holder);
            public bool IsHeldBy (object holder) => holders.Contains(holder);
            public void AddHoldersFrom (LoadedResource resource) => holders.UnionWith(resource.holders);
        }

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
        protected readonly List<LoadedResource> LoadedResources = new List<LoadedResource>();
        protected readonly IHoldersTracker HoldersTracker;

        public ResourceLoader (IList<ProvisionSource> provisionSources, IHoldersTracker holdersTracker)
        {
            ProvisionSources.AddRange(provisionSources);
            HoldersTracker = holdersTracker;
        }

        public ResourceLoader (IList<IResourceProvider> providersList, IHoldersTracker holdersTracker, string pathPrefix = null)
        {
            foreach (var provider in providersList)
                ProvisionSources.Add(new ProvisionSource(provider, pathPrefix));
            HoldersTracker = holdersTracker;
        }

        public string GetLocalPath (Resource resource)
        {
            var fullPath = resource.Path;
            return GetLocalPath(fullPath);
        }

        public virtual void Hold (string path, object holder)
        {
            var resource = GetLoadedResource(path);
            if (resource is null || !resource.Valid) return;

            resource.AddHolder(holder);

            if (resource.HoldersCount == 1)
                HoldersTracker.Hold(resource.Object, this);
        }

        public virtual void Release (string path, object holder, bool unload = true)
        {
            var resource = GetLoadedResource(path);
            if (resource is null || !resource.Valid) return;

            resource.RemoveHolder(holder);

            if (resource.HoldersCount == 0)
                if (HoldersTracker.Release(resource.Object, this) == 0 && unload)
                    Unload(path);
        }

        public virtual void ReleaseAll (object holder, bool unload = true)
        {
            var pathsToRelease = LoadedResources
                .Where(r => r.IsHeldBy(holder))
                .Select(r => r.LocalPath).ToList();
            foreach (var path in pathsToRelease)
                Release(path, holder, unload);
        }

        public virtual bool IsHeldBy (string path, object holder)
        {
            return GetLoadedResource(path)?.IsHeldBy(holder) ?? false;
        }

        public int CountHolders (string path)
        {
            return GetLoadedResource(path)?.HoldersCount ?? 0;
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
                LoadedResources.Add(new LoadedResource(resource, source));
                return resource;
            }

            return Resource<TResource>.Invalid;
        }

        public virtual async UniTask<IReadOnlyCollection<Resource<TResource>>> LoadAllAsync (string path = null)
        {
            var result = new List<Resource<TResource>>();
            var addedPaths = new HashSet<string>();
            var loadTasks = new List<UniTask<Resource<TResource>>>();
            var loadData = new Dictionary<string, (ProvisionSource, string)>();

            foreach (var source in ProvisionSources)
            {
                var fullPath = source.BuildFullPath(path);
                var locatedResourcePaths = await source.Provider.LocateResourcesAsync<TResource>(fullPath);
                foreach (var locatedResourcePath in locatedResourcePaths)
                {
                    var localPath = source.BuildLocalPath(locatedResourcePath);

                    if (addedPaths.Contains(localPath)) continue;
                    addedPaths.Add(localPath);

                    if (IsLoaded(localPath))
                    {
                        result.Add(GetLoadedOrNull(localPath));
                        continue;
                    }

                    loadTasks.Add(source.Provider.LoadResourceAsync<TResource>(locatedResourcePath));
                    loadData[locatedResourcePath] = (source, localPath);
                }
            }

            var resources = await UniTask.WhenAll(loadTasks);

            foreach (var resource in resources)
            {
                var (source, _) = loadData[resource.Path];
                LoadedResources.Add(new LoadedResource(resource, source));
                result.Add(resource);
            }

            return result;
        }

        public virtual IReadOnlyCollection<Resource<TResource>> GetAllLoaded ()
        {
            return LoadedResources.Where(r => r.Valid).Select(r => r.Resource).ToArray();
        }

        public virtual async UniTask<IReadOnlyCollection<string>> LocateAsync (string path = null)
        {
            var tasks = new List<UniTask<IEnumerable<string>>>();

            foreach (var source in ProvisionSources)
            {
                var fullPath = source.BuildFullPath(path);
                tasks.Add(source.Provider.LocateResourcesAsync<TResource>(fullPath)
                    .ContinueWith(ps => ps.Select(p => source.BuildLocalPath(p))));
            }

            var result = await UniTask.WhenAll(tasks);

            return result.SelectMany(s => s).Distinct().ToArray();
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
        }

        public virtual void UnloadAll ()
        {
            foreach (var resource in LoadedResources)
                resource.ProvisionSource.Provider.UnloadResource(resource.FullPath);
            LoadedResources.Clear();
        }

        /// <summary>
        /// Given resource with the provided local is loaded, returns full path of the resource, null otherwise.
        /// </summary>
        protected virtual string GetFullPath (string localPath)
        {
            return GetLoadedResource(localPath)?.FullPath;
        }

        /// <summary>
        /// Given resource with the provided full path is loaded, returns local path of the resource, null otherwise.
        /// </summary>
        protected virtual string GetLocalPath (string fullPath)
        {
            return LoadedResources.FirstOrDefault(r => r.FullPath.EqualsFast(fullPath))?.LocalPath;
        }

        protected virtual LoadedResource GetLoadedResource (string localPath)
        {
            return LoadedResources.FirstOrDefault(r => r.Valid && r.LocalPath.EqualsFast(localPath));
        }

        Resource IResourceLoader.GetLoadedOrNull (string path) => GetLoadedOrNull(path);
        IReadOnlyCollection<Resource> IResourceLoader.GetAllLoaded () => GetAllLoaded();
        async UniTask<Resource> IResourceLoader.LoadAsync (string path) => await LoadAsync(path);
        async UniTask<IReadOnlyCollection<Resource>> IResourceLoader.LoadAllAsync (string path) => await LoadAllAsync(path);
    }
}
