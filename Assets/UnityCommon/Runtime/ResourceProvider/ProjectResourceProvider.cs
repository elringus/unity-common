using System;
using UnityEngine;

/// <summary>
/// Provides resources stored in 'Resources' folders of the project.
/// </summary>
public class ProjectResourceProvider : MonoRunnerResourceProvider
{
    public override AsyncRunner CreateLoadRunner<T> (string path, Action<string, T> onLoaded = null)
    {
        var loadRequest = Resources.LoadAsync<T>(path);
        return new ResourceRequestRunner<T>(loadRequest, path, this, onLoaded);
    }

    public override T GetResourceBlocking<T> (string path)
    {
        return Resources.Load<T>(path);
    }

    public override void UnloadResource (string path, UnityEngine.Object resource)
    {
        if (resource) Resources.UnloadAsset(resource);
    }
}
