using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// <see cref="MonoBehaviour"/> based <see cref="IResourceProvider"/> implementation;
/// using <see cref="AsyncRunner"/>-derived classes for resource loading operations.
/// </summary>
[SpawnOnContextResolve(HideFlags.DontSave, true)]
public abstract class MonoRunnerResourceProvider : MonoBehaviour, IResourceProvider
{
    public event Action<float> OnLoadProgress;

    public bool IsLoading { get { return LoadProgress < 1f; } }
    public float LoadProgress { get; private set; }

    protected Dictionary<string, Resource> Resources = new Dictionary<string, Resource>();
    protected Dictionary<string, AsyncAction> Runners = new Dictionary<string, AsyncAction>();

    protected virtual void Awake ()
    {
        LoadProgress = 1f;
    }

    public virtual AsyncAction<Resource<T>> LoadResource<T> (string path) where T : class
    {
        if (Runners.ContainsKey(path))
            return Runners[path] as AsyncAction<Resource<T>>;

        if (Resources.ContainsKey(path))
            return AsyncAction<Resource<T>>.CreateCompleted(Resources[path] as Resource<T>);

        var resource = new Resource<T>(path);
        Resources.Add(path, resource);

        var loadRunner = CreateLoadRunner(resource);
        loadRunner.OnCompleted += HandleResourceLoaded;
        Runners.Add(path, loadRunner);
        UpdateLoadProgress();

        RunLoader(loadRunner);

        return loadRunner;
    }

    public virtual AsyncAction<List<Resource<T>>> LoadResources<T> (string path) where T : class
    {
        return LocateResources<T>(path).ThenAsync(LoadLocatedResources);
    }

    public virtual void UnloadResource (string path)
    {
        if (!ResourceLoaded(path)) return;

        if (Runners.ContainsKey(path))
            CancelResourceLoading(path);

        var resource = Resources[path];
        Resources.Remove(path);
        UnloadResource(resource);
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

    public virtual AsyncAction<bool> ResourceExists<T> (string path) where T : class
    {
        // TODO: Check for resource type.
        if (ResourceLoaded(path)) return AsyncAction<bool>.CreateCompleted(true);
        var folderPath = path.Contains("/") ? path.GetBeforeLast("/") : string.Empty;
        return LocateResources<T>(folderPath).ThenAsync(resources =>
            AsyncAction<bool>.CreateCompleted(resources.Exists(r => r.Path.Equals(path))));
    }

    public virtual AsyncAction<List<Resource<T>>> LocateResources<T> (string path) where T : class
    {
        if (path == null) path = string.Empty;

        if (Runners.ContainsKey(path))
            return Runners[path] as AsyncAction<List<Resource<T>>>;

        var locateRunner = CreateLocateRunner<T>(path);
        locateRunner.OnCompleted += locatedResources => HandleResourcesLocated(locatedResources, path);
        Runners.Add(path, locateRunner);
        UpdateLoadProgress();

        RunLocator(locateRunner);

        return locateRunner;
    }

    protected abstract AsyncRunner<Resource<T>> CreateLoadRunner<T> (Resource<T> resource) where T : class;
    protected abstract AsyncRunner<List<Resource<T>>> CreateLocateRunner<T> (string path) where T : class;
    protected abstract void UnloadResource (Resource resource);

    protected virtual void RunLoader<T> (AsyncRunner<Resource<T>> loader) where T : class
    {
        loader.Run();
    }

    protected virtual void RunLocator<T> (AsyncRunner<List<Resource<T>>> locator) where T : class
    {
        locator.Run();
    }

    protected virtual void CancelResourceLoading (string path)
    {
        if (!Runners.ContainsKey(path)) return;

        //Runners[path].Stop(); Unity .NET4.6 won't allow AsyncRunner<Resource<T>> cast to AsyncRunner<Resource>; waiting for fix.
        Runners[path].Reset();
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

    protected virtual AsyncAction<List<Resource<T>>> LoadLocatedResources<T> (List<Resource<T>> locatedResources) where T : class
    {
        // Handle corner case when resources got loaded while locating.
        foreach (var locatedResource in locatedResources)
            if (!Resources.ContainsKey(locatedResource.Path) && locatedResource.IsValid)
                Resources.Add(locatedResource.Path, locatedResource);

        var loadRunners = locatedResources.Select(r => LoadResource<T>(r.Path)).ToArray();
        var loadAction = new AsyncAction<List<Resource<T>>>(loadRunners.Select(r => r.Result).ToList());
        new AsyncActionSet(loadRunners).Then(loadAction.CompleteInstantly);

        return loadAction;
    }

    protected virtual void UpdateLoadProgress ()
    {
        var prevProgress = LoadProgress;
        if (Runners.Count == 0) LoadProgress = 1f;
        else LoadProgress = Mathf.Min(1f / Runners.Count, .999f);
        if (prevProgress != LoadProgress) OnLoadProgress.SafeInvoke(LoadProgress);
    }
}
