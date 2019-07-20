using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UnityCommon
{
    public static class ProviderListExtensions
    {
        /// <summary>
        /// Checks whether any provider in the list is loading resources.
        /// </summary>
        public static bool AnyIsLoading (this List<IResourceProvider> providers)
        {
            foreach (var provider in providers)
                if (provider.IsLoading) return true;
            return false;
        }

        /// <summary>
        /// Returns all the resources loaded by all the providers in the list.
        /// </summary>
        public static IEnumerable<Resource> GetLoadedResources (this List<IResourceProvider> providers)
        {
            return providers.SelectMany(p => p.LoadedResources);
        }

        /// <summary>
        /// Attempts to retrieve a loaded resource with the provided path; returns null if the resource is not loaded by any of the provider in the list.
        /// When resources with equal paths are loaded by multiple providers, will get the one from the higher-priority provider.
        /// </summary>
        public static Resource<T> GetLoadedResourceOrNull<T> (this List<IResourceProvider> providers, string path) where T : UnityEngine.Object
        {
            foreach (var provider in providers)
                if (provider.ResourceLoaded(path))
                    return provider.GetLoadedResourceOrNull<T>(path);
            return null;
        }

        /// <summary>
        /// Loads a resource at the provided path.
        /// When resources with equal paths are available in multiple providers, will load the one from the higher-priority provider.
        /// </summary>
        public static async Task<Resource<T>> LoadResourceAsync<T> (this List<IResourceProvider> providers, string path) where T : UnityEngine.Object
        {
            if (providers.Count == 1)
                return await providers[0].LoadResourceAsync<T>(path);
            else
            {
                foreach (var provider in providers)
                {
                    if (!await provider.ResourceExistsAsync<T>(path)) continue;
                    return await provider.LoadResourceAsync<T>(path);
                }
            }
            return new Resource<T>(path, null, null);
        }

        /// <summary>
        /// Loads all the resources at the provided path from all the providers.
        /// When a resource is available in multiple providers, will only load the one from the higher-priority provider.
        /// </summary>
        public static async Task<IEnumerable<Resource<T>>> LoadResourcesAsync<T> (this List<IResourceProvider> providers, string path) where T : UnityEngine.Object
        {
            var resources = new List<Resource<T>>();
            if (providers.Count == 1)
                resources.AddRange(await providers[0].LoadResourcesAsync<T>(path));
            else
            {
                foreach (var provider in providers)
                {
                    var locatedResourcePaths = await provider.LocateResourcesAsync<T>(path);
                    foreach (var locatedResourcePath in locatedResourcePaths)
                        if (!resources.Any(r => r.Path.EqualsFast(locatedResourcePath)))
                            resources.Add(await provider.LoadResourceAsync<T>(locatedResourcePath));
                }
            }
            return resources;
        }

        /// <summary>
        /// Locates all the resources at the provided path from all the providers.
        /// When a resource is available in multiple providers, will only get the one from the higher-priority provider.
        /// </summary>
        public static async Task<IEnumerable<string>> LocateResourcesAsync<T> (this List<IResourceProvider> providers, string path) where T : UnityEngine.Object
        {
            var result = new List<string>();
            foreach (var provider in providers)
            {
                var locatedResourcePaths = await provider.LocateResourcesAsync<T>(path);
                foreach (var locatedResourcePath in locatedResourcePaths)
                    if (!result.Any(p => p.EqualsFast(locatedResourcePath)))
                        result.Add(locatedResourcePath);
            }
            return result;
        }

        /// <summary>
        /// Locates all the folders at the provided path from all the providers.
        /// When a folder is available in multiple providers, will only get the one from the higher-priority provider.
        /// </summary>
        public static async Task<IEnumerable<Folder>> LocateFoldersAsync (this List<IResourceProvider> providers, string path)
        {
            var result = new List<Folder>();
            foreach (var provider in providers)
            {
                var locatedFolders = await provider.LocateFoldersAsync(path);
                foreach (var locatedFolder in locatedFolders)
                    if (!result.Any(r => r.Path.EqualsFast(locatedFolder.Path)))
                        result.Add(locatedFolder);
            }
            return result;
        }

        /// <summary>
        /// Checks whether a resource at the provided path exists in any of the providers.
        /// </summary>
        public static async Task<bool> ResourceExistsAsync<T> (this List<IResourceProvider> providers, string path) where T : UnityEngine.Object
        {
            foreach (var provider in providers)
                if (await provider.ResourceExistsAsync<T>(path)) return true;
            return false;
        }

        /// <summary>
        /// Unloads resource at the provided path from all the providers in the list.
        /// </summary>
        /// <param name="providers">Providers list.</param>
        /// <param name="path">Path to the resource location.</param>
        public static void UnloadResource (this List<IResourceProvider> providers, string path)
        {
            foreach (var provider in providers)
                 provider.UnloadResource(path);
        }

        /// <summary>
        /// Unloads all loaded resources from all the providers in the list.
        /// </summary>
        public static void UnloadResources (this List<IResourceProvider> providers)
        {
            foreach (var provider in providers)
                 provider.UnloadResources();
        }

        /// <summary>
        /// Checks whether resource with the provided path is loaded by any of the providers in the list.
        /// </summary>
        public static bool ResourceLoaded (this List<IResourceProvider> providers, string path)
        {
            foreach (var provider in providers)
                if (provider.ResourceLoaded(path)) return true;
            return false;
        }

        /// <summary>
        /// Checks whether resource with the provided path is currently being loaded by any of the providers in the list.
        /// </summary>
        public static bool ResourceLoading (this List<IResourceProvider> providers, string path)
        {
            foreach (var provider in providers)
                if (provider.ResourceLoading(path)) return true;
            return false;
        }
    }
}
