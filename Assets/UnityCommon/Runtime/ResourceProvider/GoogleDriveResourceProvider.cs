using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Provides resources stored in Google Drive.
/// Will only work for the resources covered by the available converters; 
/// use <see cref="AddConverter{T}(IRawConverter{T})"/> to extend covered resource types.
/// </summary>
public class GoogleDriveResourceProvider : MonoRunnerResourceProvider
{
    public static string CACHE_DIR_PATH { get { return string.Concat(Application.persistentDataPath, "/GoogleDriveResourceProviderCache"); } }

    /// <summary>
    /// Path to the drive folder where resources are located.
    /// </summary>
    public string DriveRootPath { get; set; } 
    /// <summary>
    /// Limits concurrent requests count using queueing.
    /// </summary>
    public int ConcurrentRequestsLimit { get; set; }
    /// <summary>
    /// Current pending concurrent requests count.
    /// </summary>
    public int RequestsCount { get { return Runners.Count; } }

    private Dictionary<Type, IConverter> converters = new Dictionary<Type, IConverter>();
    private Queue<Action> requestQueue = new Queue<Action>();

    /// <summary>
    /// Adds a resource type converter.
    /// </summary>
    public void AddConverter<T> (IRawConverter<T> converter) where T : class
    {
        if (converters.ContainsKey(typeof(T))) return;
        converters.Add(typeof(T), converter);
    }

    public void PurgeCache ()
    {
        if (Directory.Exists(CACHE_DIR_PATH))
            Directory.Delete(CACHE_DIR_PATH, true);
        // Flush cached file writes to IndexedDB on WebGL.
        // https://forum.unity.com/threads/webgl-filesystem.294358/#post-1940712
        #if UNITY_WEBGL && !UNITY_EDITOR
        WebGLExtensions.SyncFs();
        #endif
    }

    protected override void Awake ()
    {
        base.Awake();

        PurgeCache();
        Directory.CreateDirectory(CACHE_DIR_PATH);
    }

    protected override void RunLoader<T> (AsyncRunner<Resource<T>> loader)
    {
        if (ConcurrentRequestsLimit > 0 && RequestsCount > ConcurrentRequestsLimit)
            requestQueue.Enqueue(() => loader.Run());
        else loader.Run();
    }

    protected override void RunLocator<T> (AsyncRunner<List<Resource<T>>> locator)
    {
        if (ConcurrentRequestsLimit > 0 && RequestsCount > ConcurrentRequestsLimit)
            requestQueue.Enqueue(() => locator.Run());
        else locator.Run();
    }

    protected override AsyncRunner<Resource<T>> CreateLoadRunner<T> (Resource<T> resource) 
    {
        return new GoogleDriveResourceLoader<T>(DriveRootPath, resource, ResolveConverter<T>(), this);
    }

    protected override AsyncRunner<List<Resource<T>>> CreateLocateRunner<T> (string path)
    {
        return new GoogleDriveResourceLocator<T>(DriveRootPath, path, ResolveConverter<T>(), this);
    }

    protected override void UnloadResource (Resource resource)
    {
        if (resource.IsValid && resource.IsUnityObject)
            Destroy(resource.AsUnityObject);
    }

    protected override void HandleResourceLoaded<T> (Resource<T> resource)
    {
        base.HandleResourceLoaded(resource);
        ProcessLoadQueue();
    }

    protected override void HandleResourcesLocated<T> (List<Resource<T>> locatedResources, string path)
    {
        base.HandleResourcesLocated(locatedResources, path);
        ProcessLoadQueue();
    }

    private IRawConverter<T> ResolveConverter<T> ()
    {
        var resourceType = typeof(T);
        if (!converters.ContainsKey(resourceType))
        {
            Debug.LogError(string.Format("Converter for resource of type '{0}' is not available.", resourceType.Name));
            return null;
        }
        return converters[resourceType] as IRawConverter<T>;
    }

    private void ProcessLoadQueue ()
    {
        if (requestQueue.Count == 0) return;

        requestQueue.Dequeue()();
    }
}
