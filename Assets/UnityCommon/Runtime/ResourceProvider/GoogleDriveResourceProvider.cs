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

    private Dictionary<Type, object> converters = new Dictionary<Type, object>();

    /// <summary>
    /// Adds a resource type converter.
    /// </summary>
    public void AddConverter<T> (IRawConverter<T> converter) where T : UnityEngine.Object
    {
        converters.Add(typeof(T), converter);
    }

    public override AsyncRunner CreateLoadRunner<T> (string path, Action<string, T> onLoaded = null) 
    {
        var resourceType = typeof(T);
        if (!converters.ContainsKey(resourceType))
        {
            Debug.LogError(string.Format("Converter for resource of type '{0}' is not available.", resourceType.Name));
            return null;
        }
        var converter = converters[resourceType] as IRawConverter<T>;
        return new GoogleDriveResourceLoader<T>(DriveRootPath, path, converter, this, onLoaded);
    }

    public override T GetResourceBlocking<T> (string path)
    {
        Debug.LogError("GoogleDriveResourceProvider doesn't support blocking resource loading.");
        return null;
    }

    public override void UnloadResource (string path, UnityEngine.Object resource)
    {
        if (resource) Destroy(resource);
    }
}
