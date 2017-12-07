using UnityEngine;

/// <summary>
/// Provides resources stored in 'Resources' folders of the project.
/// </summary>
public class ProjectResourceProvider : MonoRunnerResourceProvider
{
    protected override AsyncRunner CreateLoadRunner<T> (UnityResource<T> resource)
    {
        return new ProjectResourceLoader<T>(resource, this);
    }

    protected override void UnloadResource (UnityResource resource)
    {
        if (resource.IsValid) Resources.UnloadAsset(resource.Object);
    }
}
