using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Provides resources stored in Google Drive using <a href="https://github.com/Elringus/UnityGoogleDrive">UnityGoogleDrive SDK</a>.
/// </summary>
public class GoogleDriveResourceProvider : IResourceProvider
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

        var resourceLoader = new GoogleDriveResourceLoader<T>(path, onLoadComplete: onLoaded);
        loadingResources.Add(path, resourceLoader);
        UpdateLoadProgress();
        resourceLoader.Load();
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
        if (!resources.ContainsKey(path) || loadingResources.ContainsKey(path))
        {
            Debug.LogError(string.Format("Resource '{0}' is not loaded and GoogleDriveResourceProvider doesn't support blocking loading.", path));
            return null;
        }

        var resource = resources[path] as T;
        if (!resource)
        {
            Debug.LogError(string.Format("Resource '{0}' is not of type '{1}'.", path, typeof(T).Name));
            return null;
        }

        return resource;
    }

    private void CancelResourceLoading (string path)
    {
        if (!loadingResources.ContainsKey(path)) return;

        loadingResources[path].Cancel();
        loadingResources.Remove(path);

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
