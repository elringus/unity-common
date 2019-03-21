using System.Collections.Generic;
using System.Threading.Tasks;

namespace UnityCommon
{
    /// <summary>
    /// Allows working with resources using a prioritized providers list and local paths.
    /// </summary>
    public abstract class ResourceLoader
    {
        public bool IsLoadingAny => Providers.AnyIsLoading();
        public string PathPrefix { get; }

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
        public virtual string BuildFullPath (string path)
        {
            if (!string.IsNullOrWhiteSpace(PathPrefix))
            {
                if (!string.IsNullOrWhiteSpace(path)) return $"{PathPrefix}/{path}";
                else return PathPrefix;
            }
            else return path;
        }

        public abstract void Preload (string path, bool isFullPath = false);
        public abstract Task PreloadAsync (string path, bool isFullPath = false);
        public abstract bool IsLoaded (string path, bool isFullPath = false);
        public abstract void Unload (string path, bool isFullPath = false);
        public abstract Task UnloadAsync (string path, bool isFullPath = false);
        public abstract void UnloadAll ();
        public abstract Task UnloadAllAsync ();
    }

    /// <summary>
    /// Allows working with resources of specific type using a prioritized providers list and local paths.
    /// </summary>
    public class ResourceLoader<TResource> : ResourceLoader where TResource : UnityEngine.Object
    {
        public ResourceLoader (IList<IResourceProvider> providersList, string resourcePathPrefix = null)
            : base(providersList, resourcePathPrefix) { }

        public override bool IsLoaded (string path, bool isFullPath = false)
        {
            if (!isFullPath) path = BuildFullPath(path);
            return Providers.ResourceLoaded(path);
        }

        public virtual Resource<TResource> GetLoadedResourceOrNull (string path, bool isFullPath = false)
        {
            if (!isFullPath) path = BuildFullPath(path);
            return Providers.GetLoadedResourceOrNull<TResource>(path);
        }

        public virtual Resource<TResource> Load (string path, bool isFullPath = false)
        {
            if (!isFullPath) path = BuildFullPath(path);
            return Providers.LoadResource<TResource>(path);
        }

        public virtual async Task<Resource<TResource>> LoadAsync (string path, bool isFullPath = false)
        {
            if (!isFullPath) path = BuildFullPath(path);
            return await Providers.LoadResourceAsync<TResource>(path);
        }

        public virtual IEnumerable<Resource<TResource>> LoadAll (string path = null, bool isFullPath = false)
        {
            if (!isFullPath) path = BuildFullPath(path);
            return Providers.LoadResources<TResource>(path);
        }
        
        public virtual async Task<IEnumerable<Resource<TResource>>> LoadAllAsync (string path = null, bool isFullPath = false)
        {
            if (!isFullPath) path = BuildFullPath(path);
            return await Providers.LoadResourcesAsync<TResource>(path);
        }

        public virtual IEnumerable<Resource<TResource>> LocateResources (string path, bool isFullPath = false)
        {
            if (!isFullPath) path = BuildFullPath(path);
            return Providers.LocateResources<TResource>(path);
        }

        public virtual async Task<IEnumerable<Resource<TResource>>> LocateResourcesAsync (string path, bool isFullPath = false)
        {
            if (!isFullPath) path = BuildFullPath(path);
            return await Providers.LocateResourcesAsync<TResource>(path);
        }

        public virtual bool ResourceExists (string path, bool isFullPath = false)
        {
            if (!isFullPath) path = BuildFullPath(path);
            return Providers.ResourceExists<TResource>(path);
        }

        public virtual async Task<bool> ResourceExistsAsync (string path, bool isFullPath = false)
        {
            if (!isFullPath) path = BuildFullPath(path);
            return await Providers.ResourceExistsAsync<TResource>(path);
        }

        public override void Preload (string path, bool isFullPath = false)
        {
            Load(path, isFullPath);
        }

        public override async Task PreloadAsync (string path, bool isFullPath = false)
        {
            await LoadAsync(path, isFullPath);
        }

        public override void Unload (string path, bool isFullPath = false)
        {
            if (!isFullPath) path = BuildFullPath(path);
            Providers.UnloadResource(path);
        }

        public override async Task UnloadAsync (string path, bool isFullPath = false)
        {
            if (!isFullPath) path = BuildFullPath(path);
            await Providers.UnloadResourceAsync(path);
        }

        /// <summary>
        /// Unloads all the resources loaded by all the providers in the list if they start with the <see cref="ResourceLoader.PathPrefix"/>.
        /// </summary>
        public override void UnloadAll ()
        {
            if (string.IsNullOrWhiteSpace(PathPrefix))
            {
                Providers.UnloadResources();
            }
            else
            {
                foreach (var resource in Providers.GetLoadedResources())
                    if (resource.Path.StartsWithFast(PathPrefix) || resource.Path.StartsWithFast("/" + PathPrefix))
                        Providers.UnloadResource(resource.Path);
            }
        }

        /// <summary>
        /// Asynchronously unloads all the resources loaded by all the providers in the list if they start with the <see cref="ResourceLoader.PathPrefix"/>.
        /// </summary>
        public override async Task UnloadAllAsync ()
        {
            if (string.IsNullOrWhiteSpace(PathPrefix))
            {
                await Providers.UnloadResourcesAsync();
            }
            else
            {
                foreach (var resource in Providers.GetLoadedResources())
                    if (resource.Path.StartsWithFast(PathPrefix) || resource.Path.StartsWithFast("/" + PathPrefix))
                        await Providers.UnloadResourceAsync(resource.Path);
            }
        }

    }
}
