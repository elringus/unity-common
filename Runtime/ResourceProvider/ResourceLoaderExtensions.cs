using System.Collections.Generic;
using UniRx.Async;

namespace UnityCommon
{
    /// <summary>
    /// Provides extension methods for <see cref="IResourceLoader"/>.
    /// </summary>
    public static class ResourceLoaderExtensions
    {
        /// <summary>
        /// Attempts to load a resource with the provided path and holds it in case it's loaded successfully.
        /// </summary>
        public static async UniTask<Resource<TResource>> LoadAndHoldAsync<TResource> (this IResourceLoader<TResource> loader, string path, object holder)
            where TResource : UnityEngine.Object
        {
            var resource = await loader.LoadAsync(path);
            if (loader.IsLoaded(path))
                loader.Hold(path, holder);
            return resource;
        }

        /// <summary>
        /// Attempts to load all the available resources (optionally) filtered by a base path and holds each of them.
        /// </summary>
        public static async UniTask<IEnumerable<Resource>> LoadAndHoldAllAsync<TResource> (this IResourceLoader<TResource> loader, object holder, string path = null)
            where TResource : UnityEngine.Object
        {
            var resources = await loader.LoadAllAsync(path);
            foreach (var resource in resources)
            {
                var localPath = loader.GetLocalPath(resource);
                if (!string.IsNullOrEmpty(localPath))
                    loader.Hold(localPath, holder);
            }
            return resources;
        }
    }
}
