using System;
using System.Collections.Generic;
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

    private Dictionary<string, UnityResource> resources = new Dictionary<string, UnityResource>();
    private Dictionary<string, AsyncRunner> runners = new Dictionary<string, AsyncRunner>();

    public UnityResource<T> LoadResource<T> (string path) where T : UnityEngine.Object
    {
        if (resources.ContainsKey(path))
            return resources[path] as UnityResource<T>;

        var resource = new UnityResource<T>(path);
        resource.OnLoaded += HandleResourceLoaded;
        resources.Add(path, resource);

        var loadRunner = CreateLoadRunner(resource);
        runners.Add(path, loadRunner);

        UpdateLoadProgress();

        loadRunner.Run();

        return resource;
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

    protected abstract AsyncRunner CreateLoadRunner<T> (UnityResource<T> resource) where T : UnityEngine.Object;
    protected abstract void UnloadResource (UnityResource resource);

    private void CancelResourceLoading (string path)
    {
        if (!runners.ContainsKey(path)) return;

        runners[path].Stop();
        runners.Remove(path);

        UpdateLoadProgress();
    }

    private void HandleResourceLoaded<T> (string path, T resource) where T : UnityEngine.Object
    {
        if (!resource) Debug.LogError(string.Format("Resource '{0}' failed to load.", path));

        if (runners.ContainsKey(path)) runners.Remove(path);
        else Debug.LogWarning(string.Format("Load runner for resource '{0}' not found.", path));

        UpdateLoadProgress();
    }

    private void UpdateLoadProgress ()
    {
        var prevProgress = LoadProgress;
        if (runners.Count == 0) LoadProgress = 1f;
        else LoadProgress = Mathf.Min(1f / runners.Count, .999f);
        if (prevProgress != LoadProgress) OnLoadProgress.SafeInvoke(LoadProgress);
    }
}
