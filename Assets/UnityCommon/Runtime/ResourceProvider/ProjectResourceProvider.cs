
/// <summary>
/// Provides resources stored in 'Resources' folders of the project.
/// </summary>
public class ProjectResourceProvider : IResourceProvider
{
    // Register in context on initialize. Bind to a gameobject? Don'd destroy on load?

    public T GetResource<T> (string path)
    {
        throw new System.NotImplementedException();
    }

    public void LoadResource<T> (string path)
    {
        throw new System.NotImplementedException();
    }

    public void UnloadResource<T> (T resource)
    {
        throw new System.NotImplementedException();
    }
}
