#if ADDRESSABLES_AVAILABLE

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace UnityCommon
{
    public class AddressableResourceProvider : ResourceProvider
    {
        /// <summary>
        /// All the assets managed by this provider should have this label assigned, 
        /// also their addresses are expected to start with the label followed by a slash.
        /// </summary>
        public readonly string AssetsLabel;

        private List<IResourceLocation> locations;

        public AddressableResourceProvider (string assetsLabel = "UNITY_COMMON")
        {
            AssetsLabel = assetsLabel;
        }

        public override bool SupportsType<T> () => true;

        public override async Task<Resource<T>> LoadResourceAsync<T> (string path)
        {
            if (locations is null) locations = await LoadAllLocations();
            return await base.LoadResourceAsync<T>(path);
        }

        public override async Task<IEnumerable<string>> LocateResourcesAsync<T> (string path)
        {
            if (locations is null) locations = await LoadAllLocations();
            return await base.LocateResourcesAsync<T>(path);
        }

        public override async Task<IEnumerable<Folder>> LocateFoldersAsync (string path)
        {
            if (locations is null) locations = await LoadAllLocations();
            return await base.LocateFoldersAsync(path);
        }

        protected override LoadResourceRunner<T> CreateLoadResourceRunner<T> (string path)
        {
            return new AddressableResourceLoader<T>(this, path, locations, LogMessage);
        }

        protected override LocateResourcesRunner<T> CreateLocateResourcesRunner<T> (string path)
        {
            return new AddressableResourceLocator<T>(this, path, locations);
        }

        protected override LocateFoldersRunner CreateLocateFoldersRunner (string path)
        {
            return new AddressableFolderLocator(this, path, locations);
        }

        protected override void DisposeResource (Resource resource)
        {
            if (!resource.IsValid) return;

            Addressables.Release(resource.Object);
        }

        private async Task<List<IResourceLocation>> LoadAllLocations ()
        {
            var locations = await Addressables.LoadResourceLocationsAsync(AssetsLabel).Task;
            return locations?.ToList() ?? new List<IResourceLocation>();
        }
    }
}

#endif
