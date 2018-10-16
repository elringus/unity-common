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
        protected Dictionary<string, ResourceRunner> ResourceRunners = new Dictionary<string, ResourceRunner>();

        public virtual Resource<T> LoadResource<T> (string path)
        {
            if (ResourceRunners.ContainsKey(path))
            {
                Debug.LogError($"Resource `{path}` is currently loading in async mode, but is requested in blocking mode.");
                return null;
            }

            if (LoadedResources.ContainsKey(path))
                return LoadedResources[path] as Resource<T>;

            var resource = LoadResourceBlocking<T>(path);

            HandleResourceLoaded(resource);
            return resource;
        }

        public virtual async Task<Resource<T>> LoadResourceAsync<T> (string path)
        {
            if (ResourceRunners.ContainsKey(path))
                return await (ResourceRunners[path] as LoadResourceRunner<T>);

            if (LoadedResources.ContainsKey(path))
                return LoadedResources[path] as Resource<T>;

            var resource = new Resource<T>(path);
            var loadRunner = CreateLoadRunner(resource);
            ResourceRunners.Add(path, loadRunner);
            UpdateLoadProgress();

            RunLoader(loadRunner);
            await loadRunner;

            HandleResourceLoaded(loadRunner.Resource);
            return loadRunner.Resource;
        }

        public virtual IEnumerable<Resource<T>> LoadResources<T> (string path)
        {
            var loactedResources = LocateResources<T>(path);
            return LoadLocatedResources(loactedResources);
        }

        public virtual async Task<IEnumerable<Resource<T>>> LoadResourcesAsync<T> (string path)
        {
            var loactedResources = await LocateResourcesAsync<T>(path);
            return await LoadLocatedResourcesAsync(loactedResources);
        }

        public virtual void UnloadResource (string path)
        {
            if (!ResourceLoaded(path)) return;

            if (ResourceRunners.ContainsKey(path))
                CancelResourceLoading(path);

            var resource = LoadedResources[path];
            LoadedResources.Remove(path);

            UnloadResourceBlocking(resource);

            LogMessage($"Resource '{path}' unloaded.");
        }

        public virtual async Task UnloadResourceAsync (string path)
        {
            if (!ResourceLoaded(path)) return;

            if (ResourceRunners.ContainsKey(path))
                CancelResourceLoading(path);

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

        public virtual bool ResourceLoading (string path)
        {
            return ResourceRunners.ContainsKey(path);
        }

        public virtual bool ResourceExists<T> (string path)
        {
            // TODO: Check for resource type.
            if (ResourceLoaded(path)) return true;
            var folderPath = path.Contains("/") ? path.GetBeforeLast("/") : string.Empty;
            var locatedResources = LocateResources<T>(folderPath);
            return locatedResources.Any(r => r.Path.Equals(path));
        }

        public virtual async Task<bool> ResourceExistsAsync<T> (string path)
        {
            // TODO: Check for resource type.
            if (ResourceLoaded(path)) return true;
            var folderPath = path.Contains("/") ? path.GetBeforeLast("/") : string.Empty;
            var locatedResources = await LocateResourcesAsync<T>(folderPath);
            return locatedResources.Any(r => r.Path.Equals(path));
        }

        public virtual IEnumerable<Resource<T>> LocateResources<T> (string path)
        {
            if (path == null) path = string.Empty;

            if (ResourceRunners.ContainsKey(path))
            {
                Debug.LogError($"Resources `{path}` are already being located in async mode.");
                return null;
            }

            var locatedResources = LocateResourcesBlocking<T>(path);

            HandleResourcesLocated(locatedResources, path);
            return locatedResources;
        }

        public virtual async Task<IEnumerable<Resource<T>>> LocateResourcesAsync<T> (string path)
        {
            if (path == null) path = string.Empty;

            if (ResourceRunners.ContainsKey(path))
                return await (ResourceRunners[path] as LocateResourcesRunner<T>);

            var locateRunner = CreateLocateRunner<T>(path);
            ResourceRunners.Add(path, locateRunner);
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

        protected abstract Resource<T> LoadResourceBlocking<T> (string path);
        protected abstract IEnumerable<Resource<T>> LocateResourcesBlocking<T> (string path);
        protected abstract LoadResourceRunner<T> CreateLoadRunner<T> (Resource<T> resource);
        protected abstract LocateResourcesRunner<T> CreateLocateRunner<T> (string path);
        protected abstract void UnloadResourceBlocking (Resource resource);
        protected abstract Task UnloadResourceAsync (Resource resource);

        protected virtual void RunLoader<T> (LoadResourceRunner<T> loader)
        {
            loader.Run();
        }

        protected virtual void RunLocator<T> (LocateResourcesRunner<T> locator)
        {
            locator.Run();
        }

        protected virtual void CancelResourceLoading (string path)
        {
            if (!ResourceRunners.ContainsKey(path)) return;

            ResourceRunners[path].Cancel();
            ResourceRunners.Remove(path);

            UpdateLoadProgress();
        }

        protected virtual void HandleResourceLoaded<T> (Resource<T> resource)
        {
            if (!resource.IsValid) Debug.LogError($"Resource '{resource.Path}' failed to load.");
            else LoadedResources[resource.Path] = resource;

            if (ResourceRunners.ContainsKey(resource.Path))
                ResourceRunners.Remove(resource.Path);

            UpdateLoadProgress();
        }

        protected virtual void HandleResourcesLocated<T> (IEnumerable<Resource<T>> locatedResources, string path)
        {
            if (ResourceRunners.ContainsKey(path))
                ResourceRunners.Remove(path);

            UpdateLoadProgress();
        }

        protected virtual IEnumerable<Resource<T>> LoadLocatedResources<T> (IEnumerable<Resource<T>> locatedResources)
        {
            var resources = locatedResources.Select(r => LoadResource<T>(r.Path));
            return resources?.ToList();
        }

        protected virtual async Task<IEnumerable<Resource<T>>> LoadLocatedResourcesAsync<T> (IEnumerable<Resource<T>> locatedResources)
        {
            // Handle corner case when resources got loaded while locating.
            foreach (var locatedResource in locatedResources)
                if (!LoadedResources.ContainsKey(locatedResource.Path) && locatedResource.IsValid)
                    LoadedResources.Add(locatedResource.Path, locatedResource);

            var resources = await Task.WhenAll(locatedResources.Select(r => LoadResourceAsync<T>(r.Path)));
            return resources?.ToList();
        }

        protected virtual void UpdateLoadProgress ()
        {
            var prevProgress = LoadProgress;
            if (ResourceRunners.Count == 0) LoadProgress = 1f;
            else LoadProgress = Mathf.Min(1f / ResourceRunners.Count, .999f);
            if (prevProgress != LoadProgress) OnLoadProgress?.Invoke(LoadProgress);
        }
    }
}
