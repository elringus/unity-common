using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Provides resources stored in 'Resources' folders of the project.
/// </summary>
public class ProjectResourceProvider : MonoRunnerResourceProvider
{
    protected override AsyncRunner<UnityResource<T>> CreateLoadRunner<T> (UnityResource<T> resource)
    {
        return new ProjectResourceLoader<T>(resource, this);
    }

    protected override AsyncAction<List<UnityResource<T>>> LocateResourcesAtPath<T> (string path)
    {
        // TODO: Make this async (if possible, LoadAllAsync doesn't exist).
        var objects = Resources.LoadAll<T>(path);
        var resources = objects.Select(r => new UnityResource<T>(string.Concat(path, "/", r.name), r));
        return new AsyncAction<List<UnityResource<T>>>(resources.ToList(), true);
    }

    protected override void UnloadResource (UnityResource resource)
    {
        if (resource.IsValid) Resources.UnloadAsset(resource.Object);
    }
}
