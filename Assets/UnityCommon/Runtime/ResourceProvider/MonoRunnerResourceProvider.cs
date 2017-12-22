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

    private Dictionary<string, Resource> resources = new Dictionary<string, Resource>();
    private Dictionary<string, AsyncRunner<Resource>> runners = new Dictionary<string, AsyncRunner<Resource>>();

    public AsyncAction<Resource<T>> LoadResource<T> (string path) where T : class
    {
        if (runners.ContainsKey(path))
            return runners[path] as AsyncAction<Resource<T>>;

        if (resources.ContainsKey(path))
            return new AsyncAction<Resource<T>>(resources[path] as Resource<T>, true);

        var resource = new Resource<T>(path);
        resources.Add(path, resource);

        var loadRunner = CreateLoadRunner(resource);
        loadRunner.OnCompleted += HandleResourceLoaded;
        runners.Add(path, loadRunner as AsyncRunner<Resource>);
        UpdateLoadProgress();
        loadRunner.Run();

        return loadRunner;
    }

    public AsyncAction<List<Resource<T>>> LoadResources<T> (string path) where T : class
    {
        return LocateResourcesAtPath<T>(path).ThenAsync(HandleResourcesLocated);
    }

    public void UnloadResource (string path)
    {
        if (!resources.ContainsKey(path))
        {
            Debug.LogWarning(string.Format("Resource '{0}' can't be unloaded because it's not loaded.", path));
            return;
        }

        if (runners.ContainsKey(path))
            CancelResourceLoading(path);

        var resource = resources[path];
        resources.Remove(path);
        UnloadResource(resource);
    }

    public bool ResourceExists (string path)
    {
        return resources.ContainsKey(path);
    }

    protected abstract AsyncRunner<Resource<T>> CreateLoadRunner<T> (Resource<T> resource) where T : class;
    protected abstract AsyncAction<List<Resource<T>>> LocateResourcesAtPath<T> (string path) where T : class;
    protected abstract void UnloadResource (Resource resource);

    private void CancelResourceLoading (string path)
    {
        if (!runners.ContainsKey(path)) return;

        runners[path].Stop();
        runners.Remove(path);

        UpdateLoadProgress();
    }

    private void HandleResourceLoaded<T> (Resource<T> resource) where T : class
    {
        if (!resource.IsValid) Debug.LogError(string.Format("Resource '{0}' failed to load.", resource.Path));

        if (runners.ContainsKey(resource.Path)) runners.Remove(resource.Path);
        else Debug.LogWarning(string.Format("Load runner for resource '{0}' not found.", resource.Path));

        UpdateLoadProgress();
    }

    private AsyncAction<List<Resource<T>>> HandleResourcesLocated<T> (List<Resource<T>> locatedResources) where T : class
    {
        // Handle corner case when resources got loaded while locating.
        foreach (var locatedResource in locatedResources)
            if (!resources.ContainsKey(locatedResource.Path) && locatedResource.IsValid)
                resources.Add(locatedResource.Path, locatedResource);

        var loadRunners = locatedResources.Select(r => LoadResource<T>(r.Path)).ToArray();
        var loadAction = new AsyncAction<List<Resource<T>>>(loadRunners.Select(r => r.Result).ToList());
        new AsyncActionSet(loadRunners).Then(loadAction.CompleteInstantly);

        return loadAction;
    }

    private void UpdateLoadProgress ()
    {
        var prevProgress = LoadProgress;
        if (runners.Count == 0) LoadProgress = 1f;
        else LoadProgress = Mathf.Min(1f / runners.Count, .999f);
        if (prevProgress != LoadProgress) OnLoadProgress.SafeInvoke(LoadProgress);
    }
}
