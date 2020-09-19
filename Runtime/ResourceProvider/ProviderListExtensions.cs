using System.Collections.Generic;
using System.Linq;
using UniRx.Async;

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
        public static (Resource<T>, IResourceProvider) GetLoadedResourceOrNull<T> (this List<IResourceProvider> providers, string path) 
            where T : UnityEngine.Object
        {
            foreach (var provider in providers)
                if (provider.ResourceLoaded(path))
                    return (provider.GetLoadedResourceOrNull<T>(path), provider);
            return (null, null);
        }

        /// <summary>
        /// Loads a resource at the provided path.
        /// When resources with equal paths are available in multiple providers, will load the one from the higher-priority provider.
        /// </summary>
        public static async UniTask<(Resource<T>, IResourceProvider)> LoadResourceAsync<T> (this List<IResourceProvider> providers, string path)
            where T : UnityEngine.Object
        {
            foreach (var provider in providers)
            {
                if (!await provider.ResourceExistsAsync<T>(path)) continue;
                return (await provider.LoadResourceAsync<T>(path), provider);
            }
            return (Resource<T>.Invalid, null);
        }

        /// <summary>
        /// Loads all the resources at the provided path from all the providers.
        /// When a resource is available in multiple providers, will only load the one from the higher-priority provider.
        /// </summary>
        public static async UniTask<IEnumerable<(Resource<T>, IResourceProvider)>> LoadResourcesAsync<T> (this List<IResourceProvider> providers, string path)
            where T : UnityEngine.Object
        {
            var result = new List<(Resource<T>, IResourceProvider)>();
            foreach (var provider in providers)
            {
                var locatedResourcePaths = await provider.LocateResourcesAsync<T>(path);
                foreach (var locatedResourcePath in locatedResourcePaths)
                    if (!result.Any(tuple => tuple.Item1.Path.EqualsFast(locatedResourcePath)))
                        result.Add((await provider.LoadResourceAsync<T>(locatedResourcePath), provider));
            }
            return result;
        }

        /// <summary>
        /// Locates all the resources at the provided path from all the providers.
        /// When a resource is available in multiple providers, will only get the one from the higher-priority provider.
        /// </summary>
        public static async UniTask<IEnumerable<(string, IResourceProvider)>> LocateResourcesAsync<T> (this List<IResourceProvider> providers, string path) 
            where T : UnityEngine.Object
        {
            var result = new List<(string, IResourceProvider)>();
            foreach (var provider in providers)
            {
                var locatedResourcePaths = await provider.LocateResourcesAsync<T>(path);
                foreach (var locatedResourcePath in locatedResourcePaths)
                    if (!result.Any(tuple => tuple.Item1.EqualsFast(locatedResourcePath)))
                        result.Add((locatedResourcePath, provider));
            }
            return result;
        }

        /// <summary>
        /// Locates all the folders at the provided path from all the providers.
        /// When a folder is available in multiple providers, will only get the one from the higher-priority provider.
        /// </summary>
        public static async UniTask<IEnumerable<(Folder, IResourceProvider)>> LocateFoldersAsync (this List<IResourceProvider> providers, string path)
        {
            var result = new List<(Folder, IResourceProvider)>();
            foreach (var provider in providers)
            {
                var locatedFolders = await provider.LocateFoldersAsync(path);
                foreach (var locatedFolder in locatedFolders)
                    if (!result.Any(tuple => tuple.Item1.Path.EqualsFast(locatedFolder.Path)))
                        result.Add((locatedFolder, provider));
            }
            return result;
        }

        /// <summary>
        /// Checks whether a resource at the provided path exists in any of the providers.
        /// </summary>
        public static async UniTask<(bool, IResourceProvider)> ResourceExistsAsync<T> (this List<IResourceProvider> providers, string path) 
            where T : UnityEngine.Object
        {
            foreach (var provider in providers)
                if (await provider.ResourceExistsAsync<T>(path)) return (true, provider);
            return (false, null);
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
