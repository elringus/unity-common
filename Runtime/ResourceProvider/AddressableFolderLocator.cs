#if ADDRESSABLES_AVAILABLE

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace UnityCommon
{
    public class AddressableFolderLocator : LocateFoldersRunner
    {
        private readonly List<IResourceLocation> locations;

        public AddressableFolderLocator (AddressableResourceProvider provider, string resourcePath, List<IResourceLocation> locations)
            : base(provider, resourcePath)
        {
            this.locations = locations;
        }

        public override Task RunAsync ()
        {
            var locatedResourcePaths = locations
                .Select(l => l.PrimaryKey.GetAfterFirst("/")) // Remove the addressables prefix.
                .LocateFolderPathsAtFolder(Path)
                .Select(p => new Folder(p));
            SetResult(locatedResourcePaths);

            return Task.CompletedTask;
        }
    }
}

#endif
