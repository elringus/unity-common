#if ADDRESSABLES_AVAILABLE

using System;
using System.Collections.Generic;
using UniRx.Async;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace UnityCommon
{
    public class AddressableResourceLoader<TResource> : LoadResourceRunner<TResource> 
        where TResource : UnityEngine.Object
    {
        private readonly List<IResourceLocation> locations;
        private readonly Action<string> logAction;
        private readonly string resourceAddress;

        public AddressableResourceLoader (AddressableResourceProvider provider, string resourcePath, 
            List<IResourceLocation> locations, Action<string> logAction) : base(provider, resourcePath)
        {
            this.locations = locations;
            this.logAction = logAction;
            resourceAddress = $"{provider.AssetsLabel}/{Path}";
        }

        public override async UniTask RunAsync ()
        {
            var startTime = Time.time;
            var asset = default(TResource);

            // Checking the location first (it throws an exception when loading non-existent assets).
            if (locations.Exists(l => l.PrimaryKey.EqualsFast(resourceAddress)))
            {
                var task = Addressables.LoadAssetAsync<TResource>(resourceAddress);
                while (!task.IsDone) // When awaiting the method directly it fails on WebGL (they're using mutlithreaded Task fot GetAwaiter)
                    await AsyncUtils.WaitEndOfFrame;
                asset = task.Result;
            }

            var result = new Resource<TResource>(Path, asset, Provider);
            SetResult(result);

            logAction?.Invoke($"Resource '{Path}' loaded over {Time.time - startTime:0.###} seconds.");
        }
    }
}

#endif
