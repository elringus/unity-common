
public class RemoteResourceProvider : IResourceProvider
{
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
