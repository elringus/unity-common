#if UNITY_GOOGLE_DRIVE_AVAILABLE

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityGoogleDrive;

namespace UnityCommon
{
    /// <summary>
    /// Provides resources stored in Google Drive.
    /// Will only work for the resources covered by the available converters; 
    /// use <see cref="AddConverter{T}(IRawConverter{T})"/> to extend covered resource types.
    /// </summary>
    public class GoogleDriveResourceProvider : ResourceProvider
    {
        [Serializable]
        public class CacheManifest : SerializableLiteralStringMap
        {
            public string StartToken { get { return ContainsKey(startTokenKey) ? this[startTokenKey] : null; } set { this[startTokenKey] = value; } }

            private const string startTokenKey = "GDRIVE_CACHE_START_TOKEN";
            private static readonly string filePath = string.Concat(CacheDirPath, "/CacheManifest");

            public static async Task<CacheManifest> ReadOrCreateAsync ()
            {
                if (!File.Exists(filePath))
                {
                    var manifest = new CacheManifest();
                    await IOUtils.WriteTextFileAsync(filePath, JsonUtility.ToJson(manifest));
                    return manifest;
                }

                var manifestJson = await IOUtils.ReadTextFileAsync(filePath);
                return JsonUtility.FromJson<CacheManifest>(manifestJson);
            }

            public async Task WriteAsync ()
            {
                var manifestJson = JsonUtility.ToJson(this);
                await IOUtils.WriteTextFileAsync(filePath, manifestJson);
            }
        }

        public enum CachingPolicyType { Smart, PurgeAllOnInit }

        /// <summary>
        /// Full path to the cache directory.
        /// </summary>
        public static readonly string CacheDirPath = string.Concat(Application.persistentDataPath, "/GoogleDriveResourceProviderCache");
        /// <summary>
        /// String used to replace slashes in file paths.
        /// </summary>
        public const string SlashReplace = "@@";

        /// <summary>
        /// Path to the drive folder where resources are located.
        /// </summary>
        public readonly string DriveRootPath;
        /// <summary>
        /// Limits concurrent requests count using queueing.
        /// </summary>
        public readonly int ConcurrentRequestsLimit;
        /// <summary>
        /// Caching policy to use.
        /// </summary>
        public readonly CachingPolicyType CachingPolicy;
        /// <summary>
        /// Current pending concurrent requests count.
        /// </summary>
        public int RequestsCount => LoadRunners.Count + LocateRunners.Count;

        private readonly Dictionary<Type, IConverter> converters = new Dictionary<Type, IConverter>();
        private readonly Queue<Action> requestQueue = new Queue<Action>();
        private bool smartCachingScanPending;

        public GoogleDriveResourceProvider (string driveRootPath, CachingPolicyType cachingPolicy, int concurrentRequestsLimit)
        {
            DriveRootPath = driveRootPath;
            CachingPolicy = cachingPolicy;
            ConcurrentRequestsLimit = concurrentRequestsLimit;

            IOUtils.CreateDirectory(CacheDirPath);

            LogMessage($"Caching policy: {CachingPolicy}");
            if (CachingPolicy == CachingPolicyType.PurgeAllOnInit) PurgeCache();
            if (CachingPolicy == CachingPolicyType.Smart) smartCachingScanPending = true;
        }

        public override bool SupportsType<T> () => converters.ContainsKey(typeof(T));

        /// <summary>
        /// Adds a resource type converter.
        /// </summary>
        public void AddConverter<T> (IRawConverter<T> converter)
        {
            if (converters.ContainsKey(typeof(T))) return;
            converters.Add(typeof(T), converter);
            LogMessage($"Converter '{typeof(T).Name}' added.");
        }

        public void PurgeCache ()
        {
            if (Directory.Exists(CacheDirPath))
            {
                IOUtils.DeleteDirectory(CacheDirPath, true);
                IOUtils.CreateDirectory(CacheDirPath);
            }

            LogMessage("All cached resources purged.");
        }

        public void PurgeCachedResources (string resourcesPath)
        {
            if (!Directory.Exists(CacheDirPath)) return;

            resourcesPath = resourcesPath.Replace("/", SlashReplace) + SlashReplace;

            foreach (var filePath in Directory.GetFiles(CacheDirPath).Where(f => Path.GetFileName(f).StartsWith(resourcesPath)))
            {
                File.Delete(filePath);
                LogMessage($"Cached resource '{filePath}' purged.");
            }

            IOUtils.WebGLSyncFs();
        }

        public override async Task<Resource<T>> LoadResourceAsync<T> (string path)
        {
            if (smartCachingScanPending) await RunSmartCachingScanAsync();
            return await base.LoadResourceAsync<T>(path);
        }

        protected override void RunResourceLoader<T> (LoadResourceRunner<T> loader)
        {
            if (ConcurrentRequestsLimit > 0 && RequestsCount > ConcurrentRequestsLimit)
                requestQueue.Enqueue(() => loader.RunAsync().WrapAsync());
            else base.RunResourceLoader(loader);
        }

        protected override void RunResourcesLocator<T> (LocateResourcesRunner<T> locator)
        {
            if (ConcurrentRequestsLimit > 0 && RequestsCount > ConcurrentRequestsLimit)
                requestQueue.Enqueue(() => locator.RunAsync().WrapAsync());
            else base.RunResourcesLocator(locator);
        }

        protected override LoadResourceRunner<T> CreateLoadResourceRunner<T> (string path)
        {
            return new GoogleDriveResourceLoader<T>(this, DriveRootPath, path, ResolveConverter<T>(), LogMessage);
        }

        protected override LocateResourcesRunner<T> CreateLocateResourcesRunner<T> (string path)
        {
            return new GoogleDriveResourceLocator<T>(this, DriveRootPath, path, ResolveConverter<T>());
        }

        protected override LocateFoldersRunner CreateLocateFoldersRunner (string path)
        {
            return new GoogleDriveFolderLocator(this, DriveRootPath, path);
        }

        protected override void HandleResourceLoaded<T> (Resource<T> resource)
        {
            base.HandleResourceLoaded(resource);
            ProcessLoadQueue();
        }

        protected override void HandleResourcesLocated<T> (IEnumerable<string> locatedResourcePaths, string path)
        {
            base.HandleResourcesLocated<T>(locatedResourcePaths, path);
            ProcessLoadQueue();
        }

        protected override void DisposeResource (Resource resource)
        {
            if (!resource.IsValid) return;
            ObjectUtils.DestroyOrImmediate(resource.Object);
        }

        private IRawConverter<T> ResolveConverter<T> ()
        {
            var resourceType = typeof(T);
            if (!converters.ContainsKey(resourceType))
            {
                Debug.LogError($"Converter for resource of type '{resourceType.Name}' is not available.");
                return null;
            }
            return converters[resourceType] as IRawConverter<T>;
        }

        private void ProcessLoadQueue ()
        {
            if (requestQueue.Count == 0) return;

            requestQueue.Dequeue()();
        }

        private async Task RunSmartCachingScanAsync ()
        {
            smartCachingScanPending = false;

            var startTime = DateTime.Now;
            var manifest = await CacheManifest.ReadOrCreateAsync();
            LogMessage("Running smart caching scan...");

            if (!string.IsNullOrEmpty(manifest.StartToken))
                await ProcessChangesListAsync(manifest);

            var newStartToken = (await GoogleDriveChanges.GetStartPageToken().Send()).StartPageTokenValue;
            manifest.StartToken = newStartToken;
            await manifest.WriteAsync();
            LogMessage($"Updated smart cache changes token: {newStartToken}");
            LogMessage($"Finished smart caching scan in {(DateTime.Now - startTime).TotalSeconds:0.###} seconds.");
        }

        private async Task ProcessChangesListAsync (CacheManifest manifest)
        {
            var changeList = await GoogleDriveChanges.List(manifest.StartToken).Send();
            foreach (var change in changeList.Changes)
            {
                if (!manifest.ContainsKey(change.FileId)) continue;

                var filePath = string.Concat(CacheDirPath, "/", manifest[change.FileId]);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    LogMessage($"File '{filePath}' has been changed; cached version has been purged.");
                }
            }

            if (!string.IsNullOrWhiteSpace(changeList.NextPageToken))
            {
                manifest.StartToken = changeList.NextPageToken;
                await ProcessChangesListAsync(manifest);
            }

            IOUtils.WebGLSyncFs();
        }
    }
}

#endif
