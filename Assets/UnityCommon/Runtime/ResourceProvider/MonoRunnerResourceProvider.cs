using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// <see cref="MonoBehaviour"/> based <see cref="IResourceProvider"/> implementation;
/// using <see cref="AsyncRunner"/>-derived classes for resource loading operations.
/// </summary>
[ExecuteInEditMode]
public abstract class MonoRunnerResourceProvider : MonoBehaviour, IResourceProvider
{
    public event Action<float> OnLoadProgress;
    public event Action<string> OnMessage;

    public bool IsLoading => LoadProgress < 1f;
    public float LoadProgress { get; private set; }

    protected Dictionary<string, Resource> Resources = new Dictionary<string, Resource>();
    protected Dictionary<string, ResourceRunner> Runners = new Dictionary<string, ResourceRunner>();

    protected virtual void Awake ()
    {
        LoadProgress = 1f;
    }

    public virtual async Task<Resource<T>> LoadResourceAsync<T> (string path) where T : class
    {
        if (Runners.ContainsKey(path))
            return await (Runners[path] as LoadResourceRunner<T>);

        if (Resources.ContainsKey(path))
            return Resources[path] as Resource<T>;

        var resource = new Resource<T>(path);
        Resources.Add(path, resource);

        var loadRunner = CreateLoadRunner(resource);
        Runners.Add(path, loadRunner);
        UpdateLoadProgress();

        RunLoader(loadRunner);
        await loadRunner;

        HandleResourceLoaded(loadRunner.Resource);
        return loadRunner.Resource;
    }

    public virtual async Task<List<Resource<T>>> LoadResourcesAsync<T> (string path) where T : class
    {
        var loactedResources = await LocateResourcesAsync<T>(path);
        return await LoadLocatedResourcesAsync(loactedResources);
    }

    public virtual void UnloadResource (string path)
    {
        if (!ResourceLoaded(path)) return;

        if (Runners.ContainsKey(path))
            CancelResourceLoading(path);

        var resource = Resources[path];
        Resources.Remove(path);
        UnloadResource(resource);

        LogMessage(string.Format("Resource '{0}' unloaded.", path));
    }

    public virtual void UnloadResources ()
    {
        foreach (var resource in Resources.Values.ToList())
            UnloadResource(resource.Path);
    }

    public virtual bool ResourceLoaded (string path)
    {
        return Resources.ContainsKey(path);
    }

    public virtual async Task<bool> ResourceExistsAsync<T> (string path) where T : class
    {
        // TODO: Check for resource type.
        if (ResourceLoaded(path)) return true;
        var folderPath = path.Contains("/") ? path.GetBeforeLast("/") : string.Empty;
        var locatedResources = await LocateResourcesAsync<T>(folderPath);
        return locatedResources.Exists(r => r.Path.Equals(path));
    }

    public virtual async Task<List<Resource<T>>> LocateResourcesAsync<T> (string path) where T : class
    {
        if (path == null) path = string.Empty;

        if (Runners.ContainsKey(path))
            return await (Runners[path] as LocateResourcesRunner<T>);

        var locateRunner = CreateLocateRunner<T>(path);
        Runners.Add(path, locateRunner);
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

    protected abstract LoadResourceRunner<T> CreateLoadRunner<T> (Resource<T> resource) where T : class;
    protected abstract LocateResourcesRunner<T> CreateLocateRunner<T> (string path) where T : class;
    protected abstract void UnloadResource (Resource resource);

    protected virtual void RunLoader<T> (LoadResourceRunner<T> loader) where T : class
    {
        loader.Run();
    }

    protected virtual void RunLocator<T> (LocateResourcesRunner<T> locator) where T : class
    {
        locator.Run();
    }

    protected virtual void CancelResourceLoading (string path)
    {
        if (!Runners.ContainsKey(path)) return;

        Runners[path].Cancel();
        Runners.Remove(path);

        UpdateLoadProgress();
    }

    protected virtual void HandleResourceLoaded<T> (Resource<T> resource) where T : class
    {
        if (!resource.IsValid) Debug.LogError(string.Format("Resource '{0}' failed to load.", resource.Path));

        if (Runners.ContainsKey(resource.Path)) Runners.Remove(resource.Path);
        else Debug.LogWarning(string.Format("Load runner for resource '{0}' not found.", resource.Path));

        UpdateLoadProgress();
    }

    protected virtual void HandleResourcesLocated<T> (List<Resource<T>> locatedResources, string path) where T : class
    {
        if (Runners.ContainsKey(path)) Runners.Remove(path);
        else Debug.LogWarning(string.Format("Locate runner for path '{0}' not found.", path));

        UpdateLoadProgress();
    }

    protected virtual async Task<List<Resource<T>>> LoadLocatedResourcesAsync<T> (List<Resource<T>> locatedResources) where T : class
    {
        // Handle corner case when resources got loaded while locating.
        foreach (var locatedResource in locatedResources)
            if (!Resources.ContainsKey(locatedResource.Path) && locatedResource.IsValid)
                Resources.Add(locatedResource.Path, locatedResource);

        var resources = await Task.WhenAll(locatedResources.Select(r => LoadResourceAsync<T>(r.Path)));
        return resources?.ToList();
    }

    protected virtual void UpdateLoadProgress ()
    {
        var prevProgress = LoadProgress;
        if (Runners.Count == 0) LoadProgress = 1f;
        else LoadProgress = Mathf.Min(1f / Runners.Count, .999f);
        if (prevProgress != LoadProgress) OnLoadProgress.SafeInvoke(LoadProgress);
    }
}
