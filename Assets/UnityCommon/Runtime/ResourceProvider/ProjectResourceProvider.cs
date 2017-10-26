using UnityEngine;

/// <summary>
/// Provides resources stored in 'Resources' folders of the project.
/// </summary>
[SpawnOnContextResolve(HideFlags.DontSave, true)]
public class ProjectResourceProvider : MonoBehaviour, IResourceProvider
{
    public void LoadResource<T> (string path)
    {
        throw new System.NotImplementedException();
    }

    public void UnloadResource<T> (T resource)
    {
        throw new System.NotImplementedException();
    }

    public T GetResource<T> (string path)
    {
        throw new System.NotImplementedException();
    }
}
