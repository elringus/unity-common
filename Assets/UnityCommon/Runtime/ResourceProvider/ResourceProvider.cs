using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityCommon
{
    /// <summary>
    /// A base <see cref="IResourceProvider"/> implementation.
    /// </summary>
    public abstract class ResourceProvider : IResourceProvider
    {
        public event Action<float> OnLoadProgress;
        public event Action<string> OnMessage;

        public bool IsLoading => LoadProgress < 1f;
        public float LoadProgress { get; private set; } = 1f;

        protected Dictionary<string, Resource> LoadedResources = new Dictionary<string, Resource>();
        protected Dictionary<string, ResourceRunner> LoadRunners = new Dictionary<string, ResourceRunner>();
        protected Dictionary<string, ResourceRunner> LocateRunners = new Dictionary<string, ResourceRunner>();

        public virtual Resource<T> LoadResource<T> (string path) where T : UnityEngine.Object
        {
            // We're currently loading this resource in async mode; cancel and load blocking.
            if (ResourceLoading(path)) UnloadResource(path);

            if (LoadedResources.ContainsKey(path))
            {
                if (LoadedResources[path].Object?.GetType() != typeof(T)) UnloadResource(path);
                else return LoadedResources[path] as Resource<T>;
            }

            var resource = LoadResourceBlocking<T>(path);

            HandleResourceLoaded(resource);
            return resource;
        }

        public virtual async Task<Resource<T>> LoadResourceAsync<T> (string path) where T : UnityEngine.Object
        {
            if (ResourceLoading(path))
            {
                if (LoadRunners[path].ExpectedResourceType != typeof(T)) await UnloadResourceAsync(path);
                else return await (LoadRunners[path] as LoadResourceRunner<T>);
            }

            if (LoadedResources.ContainsKey(path))
            {
                if (LoadedResources[path].Object?.GetType() != typeof(T)) await UnloadResourceAsync(path);
                else return LoadedResources[path] as Resource<T>;
            }

            var resource = new Resource<T>(path);
            var loadRunner = CreateLoadRunner(resource);
            LoadRunners.Add(path, loadRunner);
            UpdateLoadProgress();

            RunLoader(loadRunner);
            await loadRunner;

            HandleResourceLoaded(loadRunner.Resource);
            return loadRunner.Resource;
        }

        public virtual IEnumerable<Resource<T>> LoadResources<T> (string path) where T : UnityEngine.Object
        {
            var loactedResources = LocateResources<T>(path);
            return LoadLocatedResources(loactedResources);
        }

        public virtual async Task<IEnumerable<Resource<T>>> LoadResourcesAsync<T> (string path) where T : UnityEngine.Object
        {
            var loactedResources = await LocateResourcesAsync<T>(path);
            return await LoadLocatedResourcesAsync(loactedResources);
        }

        public virtual void UnloadResource (string path)
        {
            if (ResourceLoading(path))
                CancelResourceLoading(path);

            if (!ResourceLoaded(path)) return;

            var resource = LoadedResources[path];
            LoadedResources.Remove(path);

            UnloadResourceBlocking(resource);

            LogMessage($"Resource '{path}' unloaded.");
        }

        public virtual async Task UnloadResourceAsync (string path)
        {

            if (ResourceLoading(path))
                CancelResourceLoading(path);

            if (!ResourceLoaded(path)) return;

            var resource = LoadedResources[path];
            LoadedResources.Remove(path);

            await UnloadResourceAsync(resource);

            LogMessage($"Resource '{path}' unloaded.");
        }

        public virtual void UnloadResources ()
        {
            var loadedPaths = LoadedResources.Values.Select(r => r.Path).ToList();
            foreach (var path in loadedPaths)
                UnloadResource(path);
        }

        public virtual async Task UnloadResourcesAsync ()
        {
            var loadedPaths = LoadedResources.Values.Select(r => r.Path).ToList();
            await Task.WhenAll(loadedPaths.Select(path => UnloadResourceAsync(path)));
        }

        public virtual bool ResourceLoaded (string path)
        {
            return LoadedResources.ContainsKey(path);
        }

        public virtual bool ResourceLoaded<T> (string path) where T : UnityEngine.Object
        {
            return ResourceLoaded(path) && LoadedResources[path].Object.GetType() == typeof(T);
        }

        public virtual bool ResourceLoading (string path)
        {
            return LoadRunners.ContainsKey(path);
        }

        public virtual bool ResourceLocating (string path)
        {
            return LocateRunners.ContainsKey(path);
        }

        public virtual bool ResourceExists<T> (string path) where T : UnityEngine.Object
        {
            if (ResourceLoaded<T>(path)) return true;
            var folderPath = path.Contains("/") ? path.GetBeforeLast("/") : string.Empty;
            var locatedResources = LocateResources<T>(folderPath);
            return locatedResources.Any(r => r.Path.Equals(path));
        }

        public virtual async Task<bool> ResourceExistsAsync<T> (string path) where T : UnityEngine.Object
        {
            if (ResourceLoaded<T>(path)) return true;
            var folderPath = path.Contains("/") ? path.GetBeforeLast("/") : string.Empty;
            var locatedResources = await LocateResourcesAsync<T>(folderPath);
            return locatedResources.Any(r => r.Path.Equals(path));
        }

        public virtual IEnumerable<Resource<T>> LocateResources<T> (string path) where T : UnityEngine.Object
        {
            if (path == null) path = string.Empty;

            // We're currently locating this resource in async mode; cancel and locate blocking.
            if (ResourceLocating(path)) CancelResourceLocating(path);

            var locatedResources = LocateResourcesBlocking<T>(path);

            HandleResourcesLocated(locatedResources, path);
            return locatedResources;
        }

        public virtual async Task<IEnumerable<Resource<T>>> LocateResourcesAsync<T> (string path) where T : UnityEngine.Object
        {
            if (path == null) path = string.Empty;

            if (ResourceLocating(path))
            {
                if (LocateRunners[path].ExpectedResourceType != typeof(T)) CancelResourceLocating(path);
                else return await (LocateRunners[path] as LocateResourcesRunner<T>);
            }

            var locateRunner = CreateLocateRunner<T>(path);
            LocateRunners.Add(path, locateRunner);
            UpdateLoadProgress();

            RunLocator(locateRunner);

            await locateRunner;
            HandleResourcesLocated(locateRunner.LocatedResources, path);
            return locateRunner.LocatedResources;
        }

        public void LogMessage (string message)
        {
            OnMessage.SafeInvoke(message);
        }

        protected abstract Resource<T> LoadResourceBlocking<T> (string path) where T : UnityEngine.Object;
        protected abstract IEnumerable<Resource<T>> LocateResourcesBlocking<T> (string path) where T : UnityEngine.Object;
        protected abstract LoadResourceRunner<T> CreateLoadRunner<T> (Resource<T> resource) where T : UnityEngine.Object;
        protected abstract LocateResourcesRunner<T> CreateLocateRunner<T> (string path) where T : UnityEngine.Object;
        protected abstract void UnloadResourceBlocking (Resource resource);
        protected abstract Task UnloadResourceAsync (Resource resource);

        protected virtual void RunLoader<T> (LoadResourceRunner<T> loader) where T : UnityEngine.Object
        {
            loader.Run();
        }

        protected virtual void RunLocator<T> (LocateResourcesRunner<T> locator) where T : UnityEngine.Object
        {
            locator.Run();
        }

        protected virtual void CancelResourceLoading (string path)
        {
            if (!ResourceLoading(path)) return;

            LoadRunners[path].Cancel();
            LoadRunners.Remove(path);

            UpdateLoadProgress();
        }

        protected virtual void CancelResourceLocating (string path)
        {
            if (!ResourceLocating(path)) return;

            LocateRunners[path].Cancel();
            LocateRunners.Remove(path);

            UpdateLoadProgress();
        }

        protected virtual void HandleResourceLoaded<T> (Resource<T> resource) where T : UnityEngine.Object
        {
            if (!resource.IsValid) Debug.LogError($"Resource '{resource.Path}' failed to load.");
            else LoadedResources[resource.Path] = resource;

            if (LoadRunners.ContainsKey(resource.Path))
                LoadRunners.Remove(resource.Path);

            UpdateLoadProgress();
        }

        protected virtual void HandleResourcesLocated<T> (IEnumerable<Resource<T>> locatedResources, string path) where T : UnityEngine.Object
        {
            if (LocateRunners.ContainsKey(path))
                LocateRunners.Remove(path);

            UpdateLoadProgress();
        }

        protected virtual IEnumerable<Resource<T>> LoadLocatedResources<T> (IEnumerable<Resource<T>> locatedResources) where T : UnityEngine.Object
        {
            return locatedResources.Select(r => LoadResource<T>(r.Path));
        }

        protected virtual async Task<IEnumerable<Resource<T>>> LoadLocatedResourcesAsync<T> (IEnumerable<Resource<T>> locatedResources) where T : UnityEngine.Object
        {
            return await Task.WhenAll(locatedResources.Select(r => LoadResourceAsync<T>(r.Path)));
        }

        protected virtual void UpdateLoadProgress ()
        {
            var prevProgress = LoadProgress;
            var runnersCount = LoadRunners.Count + LocateRunners.Count;
            if (runnersCount == 0) LoadProgress = 1f;
            else LoadProgress = Mathf.Min(1f / runnersCount, .999f);
            if (prevProgress != LoadProgress) OnLoadProgress?.Invoke(LoadProgress);
        }
    }
}
