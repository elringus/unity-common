using System.Collections.Generic;

namespace UnityCommon
{
    /// <summary>
    /// Keeps required data to construct a <see cref="ResourceLoader"/> for specific types on-site.
    /// </summary>
    public class ResourceLoaderConstructor
    {
        public readonly List<IResourceProvider> ResourceProviders;
        public readonly string ResourcePathPrefix;

        public ResourceLoaderConstructor (List<IResourceProvider> resourceProviders, string resourcePathPrefix = null)
        {
            ResourceProviders = resourceProviders;
            ResourcePathPrefix = resourcePathPrefix;
        }

        public ResourceLoader<TResource> ConstructFor<TResource> () where TResource : class
        {
            return new ResourceLoader<TResource>(ResourceProviders, ResourcePathPrefix);
        }
    }
}
