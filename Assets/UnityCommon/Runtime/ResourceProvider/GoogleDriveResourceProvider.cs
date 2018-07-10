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
    public class GoogleDriveResourceProvider : MonoRunnerResourceProvider
    {
        public enum CachingPolicyType { Smart, PurgeAllOnInit }

        public static string CacheDirPath => string.Concat(Application.persistentDataPath, "/GoogleDriveResourceProviderCache");
        public static string SmartCacheStartTokenKey => "GDRIVE_CACHE_START_TOKEN";
        public static string SmartCacheKeyPrefix => "GDRIVE_CACHE_";
        public const string SlashReplace = "@@";

        /// <summary>
        /// Path to the drive folder where resources are located.
        /// </summary>
        public string DriveRootPath { get; set; }
        /// <summary>
        /// Limits concurrent requests count using queueing.
        /// </summary>
        public int ConcurrentRequestsLimit { get; set; }
        /// <summary>
        /// Caching policy to use.
        /// </summary>
        public CachingPolicyType CachingPolicy { get; set; }
        /// <summary>
        /// Current pending concurrent requests count.
        /// </summary>
        public int RequestsCount => Runners.Count;

        private Dictionary<Type, IConverter> converters = new Dictionary<Type, IConverter>();
        private Queue<Action> requestQueue = new Queue<Action>();
        private bool smartCachingScanPending;

        /// <summary>
        /// Adds a resource type converter.
        /// </summary>
        public void AddConverter<T> (IRawConverter<T> converter) where T : class
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

        protected override void Awake ()
        {
            base.Awake();

            IOUtils.CreateDirectory(CacheDirPath);

            LogMessage($"Caching policy: {CachingPolicy}");
            if (CachingPolicy == CachingPolicyType.PurgeAllOnInit) PurgeCache();
            if (CachingPolicy == CachingPolicyType.Smart) smartCachingScanPending = true;
        }

        protected override void RunLoader<T> (LoadResourceRunner<T> loader)
        {
            if (ConcurrentRequestsLimit > 0 && RequestsCount > ConcurrentRequestsLimit)
                requestQueue.Enqueue(() => loader.Run());
            else loader.Run();
        }

        protected override void RunLocator<T> (LocateResourcesRunner<T> locator)
        {
            if (ConcurrentRequestsLimit > 0 && RequestsCount > ConcurrentRequestsLimit)
                requestQueue.Enqueue(() => locator.Run());
            else locator.Run();
        }

        protected override LoadResourceRunner<T> CreateLoadRunner<T> (Resource<T> resource)
        {
            return new GoogleDriveResourceLoader<T>(DriveRootPath, resource, ResolveConverter<T>(), LogMessage);
        }

        protected override LocateResourcesRunner<T> CreateLocateRunner<T> (string path)
        {
            return new GoogleDriveResourceLocator<T>(DriveRootPath, path, ResolveConverter<T>());
        }

        protected override void UnloadResource (Resource resource)
        {
            if (resource.IsValid && resource.IsUnityObject)
                Destroy(resource.AsUnityObject);
        }

        protected override void HandleResourceLoaded<T> (Resource<T> resource)
        {
            base.HandleResourceLoaded(resource);
            ProcessLoadQueue();
        }

        protected override void HandleResourcesLocated<T> (List<Resource<T>> locatedResources, string path)
        {
            base.HandleResourcesLocated(locatedResources, path);
            ProcessLoadQueue();
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

            var startTime = Time.time;
            LogMessage("Running smart caching scan...");

            if (PlayerPrefs.HasKey(SmartCacheStartTokenKey))
                await ProcessChangesListAsync(PlayerPrefs.GetString(SmartCacheStartTokenKey));

            var newStartToken = (await GoogleDriveChanges.GetStartPageToken().Send()).StartPageTokenValue;
            PlayerPrefs.SetString(SmartCacheStartTokenKey, newStartToken);
            LogMessage($"Updated smart cache changes token: {newStartToken}");
            LogMessage($"Finished smart caching scan in {Time.time - startTime:0.###} seconds.");
        }

        private async Task ProcessChangesListAsync (string pageToken)
        {
            var changeList = await GoogleDriveChanges.List(pageToken).Send();
            foreach (var change in changeList.Changes)
            {
                var cachedFileKey = string.Concat(SmartCacheKeyPrefix, change.FileId);
                if (PlayerPrefs.HasKey(cachedFileKey))
                {
                    var filePath = string.Concat(CacheDirPath, "/", PlayerPrefs.GetString(cachedFileKey));
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                        LogMessage($"File '{filePath}' has been changed; cached version has been purged.");
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(changeList.NextPageToken))
                await ProcessChangesListAsync(changeList.NextPageToken);

            IOUtils.WebGLSyncFs();
        }
    }
}
