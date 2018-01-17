using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Provides resources stored in Google Drive.
/// Will only work for the resources covered by the available converters; 
/// use <see cref="AddConverter{T}(IRawConverter{T})"/> to extend covered resource types.
/// </summary>
public class GoogleDriveResourceProvider : MonoRunnerResourceProvider
{
    /// <summary>
    /// Path to the drive folder where resources are located.
    /// </summary>
    public string DriveRootPath { get; set; } 
    /// <summary>
    /// Limits concurrent requests count using queueing.
    /// </summary>
    public int ConcurrentRequestsLimit { get; set; }

    private Dictionary<Type, IConverter> converters = new Dictionary<Type, IConverter>();
    private Queue<Action> loadQueue = new Queue<Action>();

    /// <summary>
    /// Adds a resource type converter.
    /// </summary>
    public void AddConverter<T> (IRawConverter<T> converter) where T : class
    {
        converters.Add(typeof(T), converter);
    }

    protected override void RunLoader<T> (AsyncRunner<Resource<T>> loader)
    {
        if (ConcurrentRequestsLimit > 0 && Runners.Count > ConcurrentRequestsLimit)
            loadQueue.Enqueue(() => loader.Run());
        else loader.Run();
    }

    protected override AsyncRunner<Resource<T>> CreateLoadRunner<T> (Resource<T> resource) 
    {
        return new GoogleDriveResourceLoader<T>(DriveRootPath, resource, ResolveConverter<T>(), this);
    }

    protected override AsyncAction<List<Resource<T>>> LocateResourcesAtPath<T> (string path)
    {
        return new GoogleDriveResourceLocator<T>(DriveRootPath, path, ResolveConverter<T>(), this).Run();
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
        if (loadQueue.Count == 0) return;

        loadQueue.Dequeue()();
    }
}
