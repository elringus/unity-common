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

    public AsyncAction<UnityResource<T>> LoadResource<T> (string path) where T : UnityEngine.Object
    {
        var resource = new UnityResource<T>(path);
        var loadAction = new AsyncAction<UnityResource<T>>(resource);

        if (resources.ContainsKey(path))
        {
            loadAction.CompleteInstantly(resources[path] as UnityResource<T>);
            return loadAction;
        }

        resources.Add(path, resource);
        loadAction.OnCompleted += HandleResourceLoaded;

        var loadRunner = CreateLoadRunner(resource);
        loadRunner.OnCompleted += loadAction.CompleteInstantly;
        runners.Add(path, loadRunner);
        UpdateLoadProgress();
        loadRunner.Run();

        return loadAction;
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

    private void HandleResourceLoaded<T> (UnityResource<T> resource) where T : UnityEngine.Object
    {
        if (!resource.IsValid) Debug.LogError(string.Format("Resource '{0}' failed to load.", resource.Path));

        if (runners.ContainsKey(resource.Path)) runners.Remove(resource.Path);
        else Debug.LogWarning(string.Format("Load runner for resource '{0}' not found.", resource.Path));

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
