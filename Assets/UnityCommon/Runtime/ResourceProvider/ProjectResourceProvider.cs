using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Provides resources stored in 'Resources' folders of the project.
/// </summary>
[SpawnOnContextResolve(HideFlags.DontSave, true)]
public class ProjectResourceProvider : MonoBehaviour, IResourceProvider
{
    public event Action<float> OnLoadProgress;

    public bool IsLoading { get { return LoadProgress < 1f; } }
    public float LoadProgress { get { return loadProgress; } }

    private Dictionary<string, UnityEngine.Object> resources = new Dictionary<string, UnityEngine.Object>();
    private Dictionary<string, AsyncRunner> loadingResources = new Dictionary<string, AsyncRunner>();
    private float loadProgress = 1f;

    public void LoadResourceAsync<T> (string path, Action<string, T> onLoaded = null) where T : UnityEngine.Object
    {
        if (resources.ContainsKey(path))
        {
            Debug.LogWarning(string.Format("Resource '{0}' won't load because it's already loaded.", path));
            return;
        }

        if (loadingResources.ContainsKey(path))
        {
            Debug.LogError(string.Format("Resource '{0}' won't load because it's already loading.", path));
            return;
        }

        var loadRequest = Resources.LoadAsync<T>(path);
        var requestRunner = new ResourceRequestRunner<T>(this, HandleResourceLoaded);
        if (onLoaded != null) requestRunner.OnLoadComplete += onLoaded;
        loadingResources.Add(path, requestRunner);

        UpdateLoadProgress();

        requestRunner.Run(loadRequest, path);
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
        Resources.UnloadAsset(resource);
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
            resource = Resources.Load<T>(path);
            if (!resource)
            {
                Debug.LogError(string.Format("Resource '{0}' not found in the project resources.", path));
                return default(T);
            }
            else resources.Add(path, resource);
        }
        else resource = resources[path];

        var castedAsset = resource as T;
        if (!castedAsset)
        {
            Debug.LogError(string.Format("Resource '{0}' is not of type '{1}'.", path, typeof(T).Name));
            return default(T);
        }

        return castedAsset;
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
                Debug.LogWarning(string.Format("Loaded resource '{0}' already exists in resources. It will be replaced.", path));
                resources[path] = resource;
            }
            else resources.Add(path, resource);
        }
        else Debug.LogError(string.Format("Resource '{0}' wasn't loaded because it's not found in the project resources.", path));

        if (!loadingResources.ContainsKey(path))
            Debug.LogWarning(string.Format("Loaded resource '{0}' not found in loading resources map.", path));
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
