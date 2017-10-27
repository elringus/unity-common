using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Provides resources stored in 'Resources' folders of the project.
/// </summary>
[SpawnOnContextResolve(HideFlags.DontSave, true)]
public class ProjectResourceProvider : MonoBehaviour, IResourceProvider
{
    public float LoadProgress { get { return EvaluateLoadProgress(); } }

    private Dictionary<string, Object> resources = new Dictionary<string, Object>();
    private Dictionary<string, ResourceRequestRunner> loadingResources = new Dictionary<string, ResourceRequestRunner>();

    public void LoadResourceAsync<T> (string path) where T : Object
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
        if (loadRequest == null)
        {
            Debug.LogError(string.Format("Resource '{0}' won't load because it's not found in the project resources.", path));
            return;
        }

        var requestRunner = new ResourceRequestRunner(this, HandleResourceLoaded);
        loadingResources.Add(path, requestRunner);
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

        var assetToUnload = resources[path];
        resources.Remove(path);
        Resources.UnloadAsset(assetToUnload);
    }

    public T GetResource<T> (string path) where T : Object
    {
        Object asset = null;

        if (loadingResources.ContainsKey(path))
        {
            Debug.LogWarning(string.Format("Resource '{0}' was required while it was still loading. Loading will be canceled.", path));
            CancelResourceLoading(path);
        }

        if (!resources.ContainsKey(path))
        {
            asset = Resources.Load<T>(path);
            if (!asset)
            {
                Debug.LogError(string.Format("Resource '{0}' not found in the project resources.", path));
                return default(T);
            }
        }
        else asset = resources[path];

        var castedAsset = asset as T;
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

        var loadingResource = loadingResources[path];
        loadingResource.Stop();
        loadingResources.Remove(path);
    }

    private float EvaluateLoadProgress ()
    {
        if (loadingResources.Count == 0) return 1f;
        return Mathf.Min(1f / loadingResources.Count, .999f);
    }

    private void HandleResourceLoaded (ResourceRequestRunner resourceRequestRunner)
    {
        if (resources.ContainsKey(resourceRequestRunner.ResourcePath))
        {
            Debug.LogWarning("HandleResourceLoaded: Loaded resource already exists in resources. It will be replaced.");
            resources[resourceRequestRunner.ResourcePath] = resourceRequestRunner.ResourceRequest.asset;
        }
        else resources.Add(resourceRequestRunner.ResourcePath, resourceRequestRunner.ResourceRequest.asset);

        if (!loadingResources.ContainsKey(resourceRequestRunner.ResourcePath))
            Debug.LogWarning("HandleResourceLoaded: Loaded resource not found in loading resources map.");
        else loadingResources.Remove(resourceRequestRunner.ResourcePath);
    }
}
