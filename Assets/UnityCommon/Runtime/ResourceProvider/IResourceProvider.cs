using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Implementation is able to load and unload <see cref="Resource"/> objects at runtime.
/// </summary>
public interface IResourceProvider
{
    /// <summary>
    /// Event executed when load progress is changed.
    /// </summary>
    event Action<float> OnLoadProgress;
    /// <summary>
    /// Event executed when an information message is sent by the provider.
    /// </summary>
    event Action<string> OnMessage;

    /// <summary>
    /// Whether any resource loading operations are currently active.
    /// </summary>
    bool IsLoading { get; }

    /// <summary>
    /// Current resources loading progress, in 0.0 to 1.0 range.
    /// </summary>
    float LoadProgress { get; }

    /// <summary>
    /// Loads resource asynchronously.
    /// </summary>
    /// <typeparam name="T">Type of the resource to load.</typeparam>
    /// <param name="path">Path to the resource location.</param>
    Task<Resource<T>> LoadResourceAsync<T> (string path) where T : class;

    /// <summary>
    /// Loads all available resources at the provided path asynchronously.
    /// </summary>
    /// <typeparam name="T">Type of the resources to load.</typeparam>
    /// <param name="path">Path to the resources location.</param>
    Task<List<Resource<T>>> LoadResourcesAsync<T> (string path) where T : class;

    /// <summary>
    /// Locates all available resources at the provided path asynchronously.
    /// </summary>
    /// <typeparam name="T">Type of the resources to locate.</typeparam>
    /// <param name="path">Path to the resources location.</param>
    Task<List<Resource<T>>> LocateResourcesAsync<T> (string path) where T : class;

    /// <summary>
    /// Checks whether resource with the provided type and path is available.
    /// </summary>
    /// <typeparam name="T">Type of the resource to look for.</typeparam>
    /// <param name="path">Path to the resource location.</param>
    Task<bool> ResourceExistsAsync<T> (string path) where T : class;

    /// <summary>
    /// Unloads resource at the provided path.
    /// </summary>
    /// <param name="path">Path to the resource location.</param>
    void UnloadResource (string path);

    /// <summary>
    /// Unloads all loaded resources.
    /// </summary>
    void UnloadResources ();

    /// <summary>
    /// Checks whether resource with the provided path is loaded.
    /// </summary>
    bool ResourceLoaded (string path);
}
