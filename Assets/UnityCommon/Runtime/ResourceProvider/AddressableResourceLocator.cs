#if ADDRESSABLES_AVAILABLE

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace UnityCommon
{
    public class AddressableResourceLocator<TResource> : LocateResourcesRunner<TResource> 
        where TResource : UnityEngine.Object
    {
        private readonly List<IResourceLocation> locations;

        public AddressableResourceLocator (AddressableResourceProvider provider, string resourcePath, List<IResourceLocation> locations) 
            : base(provider, resourcePath)
        {
            this.locations = locations;
        }

        public override Task RunAsync ()
        {
            var locatedResourcePaths = locations
                .Where(l => l.ResourceType == typeof(TResource))
                .Select(l => l.PrimaryKey.GetAfterFirst("/")) // Remove the addressables prefix.
                .LocateResourcePathsAtFolder(Path);
            SetResult(locatedResourcePaths);

            return Task.CompletedTask;
        }
    }
}

#endif
