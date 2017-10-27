using UnityEngine;

public interface IResourceProvider
{
    /// <summary>
    /// Current resources loading progress, in 0.0 to 1.0 range.
    /// </summary>
    float LoadProgress { get; }

    /// <summary>
    /// Preloads resource asynchronously.
    /// </summary>
    void LoadResourceAsync<T> (string path) where T : Object;

    /// <summary>
    /// Unloads resource asynchronously.
    /// </summary>
    void UnloadResourceAsync (string path);

    /// <summary>
    /// Attempts to retrieve previously loaded resource. 
    /// Will load (or wait for pending load operation) the resource if it's not found. 
    /// </summary>
    T GetResource<T> (string path) where T : Object;
}
