
public interface IResourceProvider
{
    /// <summary>
    /// Preloads resource asynchronously.
    /// </summary>
    void LoadResource<T> (string path);

    /// <summary>
    /// Unloads resource asynchronously.
    /// </summary>
    void UnloadResource<T> (T resource);

    /// <summary>
    /// Attempts to retrieve previously loaded resource. 
    /// Will load (or wait for pending load operation) the resource if it's not found. 
    /// </summary>
    T GetResource<T> (string path);
}
