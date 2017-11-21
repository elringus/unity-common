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
    public float LoadProgress { get { return loadProgress; } }

    private Dictionary<string, UnityEngine.Object> resources = new Dictionary<string, UnityEngine.Object>();
    private Dictionary<string, AsyncRunner> loadingResources = new Dictionary<string, AsyncRunner>();
    private float loadProgress = 1f;

    public abstract AsyncRunner CreateLoadRunner<T> (string path, Action<string, T> onLoaded = null) where T : UnityEngine.Object;
    public abstract T GetResourceBlocking<T> (string path) where T : UnityEngine.Object;
    public abstract void UnloadResource (string path, UnityEngine.Object resource);

    public void LoadResourceAsync<T> (string path, Action<string, T> onLoaded = null) where T : UnityEngine.Object
    {
        if (resources.ContainsKey(path))
        {
            Debug.LogWarning(string.Format("Resource '{0}' won't load because it's already loaded.", path));
            return;
        }

        if (loadingResources.ContainsKey(path))
        {
            Debug.LogWarning(string.Format("Resource '{0}' won't load because it's already loading.", path));
            return;
        }

        if (onLoaded != null) onLoaded += HandleResourceLoaded;
        else onLoaded = HandleResourceLoaded;
        var loadRunner = CreateLoadRunner(path, onLoaded);
        loadingResources.Add(path, loadRunner);

        UpdateLoadProgress();

        loadRunner.Run();
    }

    public void UnloadResourceAsync (string path)
    {
        if (loadingResources.ContainsKey(path))
        {
            CancelResourceLoading(path);
            return;
        }

        if (!resources.ContainsKey(path))
        {
            Debug.LogWarning(string.Format("Resource '{0}' can't be unloaded because it's not loaded.", path));
            return;
        }

        var resource = resources[path];
        resources.Remove(path);
        UnloadResource(path, resource);
    }

    public T GetResource<T> (string path) where T : UnityEngine.Object
    {
        UnityEngine.Object resource = null;

        if (loadingResources.ContainsKey(path))
        {
            Debug.LogWarning(string.Format("Resource '{0}' was required while it was still loading. Loading will be canceled.", path));
            CancelResourceLoading(path);
        }

        if (!resources.ContainsKey(path))
        {
            resource = GetResourceBlocking<T>(path);
            if (!resource)
            {
                Debug.LogError(string.Format("Resource '{0}' is unvailable.", path));
                return null;
            }
            else resources.Add(path, resource);
        }
        else resource = resources[path];

        var castedResource = resource as T;
        if (!castedResource)
        {
            Debug.LogError(string.Format("Resource '{0}' is not of type '{1}'.", path, typeof(T).Name));
            return null;
        }

        return castedResource;
    }

    private void CancelResourceLoading (string path)
    {
        if (!loadingResources.ContainsKey(path)) return;

        loadingResources[path].Cancel();
        loadingResources.Remove(path);

        UpdateLoadProgress();
    }

    private void HandleResourceLoaded<T> (string path, T resource) where T : UnityEngine.Object
    {
        if (resource)
        {
            if (resources.ContainsKey(path))
            {
                Debug.LogWarning(string.Format("Loaded resource '{0}' already exists. It will be replaced.", path));
                resources[path] = resource;
            }
            else resources.Add(path, resource);
        }
        else Debug.LogError(string.Format("Resource '{0}' failed to load.", path));

        if (!loadingResources.ContainsKey(path))
            Debug.LogWarning(string.Format("Loaded resource '{0}' not found in loading resources.", path));
        else loadingResources.Remove(path);

        UpdateLoadProgress();
    }

    private void UpdateLoadProgress ()
    {
        var prevProgress = loadProgress;
        if (loadingResources.Count == 0) loadProgress = 1f;
        else loadProgress = Mathf.Min(1f / loadingResources.Count, .999f);
        if (prevProgress != loadProgress) OnLoadProgress.SafeInvoke(loadProgress);
    }
}
