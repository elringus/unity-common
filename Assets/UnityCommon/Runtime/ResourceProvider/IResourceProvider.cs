
public interface IResourceProvider
{
    /// <summary>
    /// Preloads resource asynchronously.
    /// </summary>
    void LoadResourceAsync (string path);

    /// <summary>
    /// Unloads resource asynchronously.
    /// </summary>
    void UnloadResourceAsync (string path);

    /// <summary>
    /// Attempts to retrieve previously loaded resource. 
    /// Will load (or wait for pending load operation) the resource if it's not found. 
    /// </summary>
    T GetResource<T> (string path) where T : UnityEngine.Object;
}
