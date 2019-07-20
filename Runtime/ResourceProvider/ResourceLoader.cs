using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityCommon
{
    /// <summary>
    /// Allows working with resources using a prioritized providers list and local paths.
    /// </summary>
    public abstract class ResourceLoader
    {
        /// <summary>
        /// Whether any of the providers used by this loader is currently loading anything.
        /// </summary>
        public bool IsLoadingAny => Providers.AnyIsLoading();
        /// <summary>
        /// Prefix used by this provider to build full resource paths from provided local paths.
        /// </summary>
        public string PathPrefix { get; }

        /// <summary>
        /// Prioritized providers list used by this loader.
        /// </summary>
        protected List<IResourceProvider> Providers { get; }

        public ResourceLoader (IList<IResourceProvider> providersList, string resourcePathPrefix = null)
        {
            Providers = new List<IResourceProvider>();
            Providers.AddRange(providersList);

            PathPrefix = resourcePathPrefix;
        }

        /// <summary>
        /// Given a local path to the resource, builds full path using predefined <see cref="PathPrefix"/>.
        /// </summary>
        public string BuildFullPath (string localPath)
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
        public string BuildLocalPath (string fullPath)
        {
            if (!string.IsNullOrWhiteSpace(PathPrefix))
            {
                var prefixAndSlash = $"{PathPrefix}/";
                if (!fullPath.Contains(prefixAndSlash))
                {
                    Debug.LogError($"Failed to buil local path from `{fullPath}`: the provided path doesn't contain `{PathPrefix}` path prefix.");
                    return null;
                }
                return fullPath.GetAfterFirst(prefixAndSlash);
            }
            else return fullPath;
        }

        public abstract bool IsLoaded (string path, bool isFullPath = false);
        public abstract void Unload (string path, bool isFullPath = false);
        public abstract void UnloadAll ();
    }

    /// <summary>
    /// Allows working with resources of specific type using a prioritized providers list and local paths.
    /// </summary>
    public class ResourceLoader<TResource> : ResourceLoader where TResource : UnityEngine.Object
    {
        /// <summary>
        /// Resources loaded by this loader.
        /// </summary>
        protected readonly List<Resource<TResource>> LoadedResources = new List<Resource<TResource>>();

        public ResourceLoader (IList<IResourceProvider> providersList, string resourcePathPrefix = null)
            : base(providersList, resourcePathPrefix) { }

        public override bool IsLoaded (string path, bool isFullPath = false)
        {
            if (!isFullPath) path = BuildFullPath(path);
            return Providers.ResourceLoaded(path);
        }

        public virtual Resource<TResource> GetLoadedOrNull (string path, bool isFullPath = false)
        {
            if (!isFullPath) path = BuildFullPath(path);
            return Providers.GetLoadedResourceOrNull<TResource>(path);
        }

        public virtual async Task<Resource<TResource>> LoadAsync (string path, bool isFullPath = false)
        {
            if (!isFullPath) path = BuildFullPath(path);

            var resource = await Providers.LoadResourceAsync<TResource>(path);
            if (resource != null && resource.IsValid)
                LoadedResources.Add(resource);
            return resource;
        }

        public virtual async Task<IEnumerable<Resource<TResource>>> LoadAllAsync (string path = null, bool isFullPath = false)
        {
            if (!isFullPath) path = BuildFullPath(path);

            var resources = await Providers.LoadResourcesAsync<TResource>(path);
            foreach (var resource in resources)
                if (resource != null && resource.IsValid)
                    LoadedResources.Add(resource);
            return resources;
        }

        public virtual async Task<IEnumerable<string>> LocateAsync (string path, bool isFullPath = false)
        {
            if (!isFullPath) path = BuildFullPath(path);
            return await Providers.LocateResourcesAsync<TResource>(path);
        }

        public virtual async Task<bool> ExistsAsync (string path, bool isFullPath = false)
        {
            if (!isFullPath) path = BuildFullPath(path);
            return await Providers.ResourceExistsAsync<TResource>(path);
        }

        public override void Unload (string path, bool isFullPath = false)
        {
            if (!isFullPath) path = BuildFullPath(path);

            Providers.UnloadResource(path);
            LoadedResources.RemoveAll(r => r is null || r.Path.EqualsFast(path));
        }

        /// <summary>
        /// Unloads all the resources previously loaded by this loader.
        /// </summary>
        public override void UnloadAll ()
        {
            foreach (var resource in LoadedResources)
                resource.Provider.UnloadResource(resource.Path);
            LoadedResources.Clear();
        }

        /// <summary>
        /// Retrieves all the resources loaded by this loader.
        /// </summary>
        public virtual List<Resource<TResource>> GetAllLoaded () => LoadedResources.Where(r => r != null).ToList();
    }
}
