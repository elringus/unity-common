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
    private struct LoadRequest { public AsyncAction Action; public Type ResourceType; public string Path; }

    /// <summary>
    /// Path to the drive folder where resources are located.
    /// </summary>
    public string DriveRootPath { get; set; } 
    /// <summary>
    /// Limits the request rate per second using queueing.
    /// </summary>
    public int RequestPerSecondLimit { get; set; }

    private Dictionary<Type, IConverter> converters = new Dictionary<Type, IConverter>();
    private Queue<LoadRequest> loadQueue = new Queue<LoadRequest>();

    public override AsyncAction<Resource<T>> LoadResource<T> (string path)
    {
        if (ResourceExists(path))
            return base.LoadResource<T>(path);

        if (RequestPerSecondLimit > 0 && Runners.Count >= RequestPerSecondLimit)
        {
            var action = new AsyncAction<Resource<T>>();
            var request = new LoadRequest() { Action = action, Path = path, ResourceType = typeof(T) };
            loadQueue.Enqueue(request);
            return action;
        }
        else return base.LoadResource<T>(path);
    }

    /// <summary>
    /// Adds a resource type converter.
    /// </summary>
    public void AddConverter<T> (IRawConverter<T> converter) where T : class
    {
        converters.Add(typeof(T), converter);
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

        var request = loadQueue.Dequeue();
        var method = typeof(GoogleDriveResourceProvider).GetMethod("LoadResource");
        var generic = method.MakeGenericMethod(request.ResourceType);
        var action = generic.Invoke(this, new object[] { request.Path }) as AsyncAction;
        action.Then(request.Action.CompleteInstantly);
    }
}
