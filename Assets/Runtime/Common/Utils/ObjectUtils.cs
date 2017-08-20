using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Object = UnityEngine.Object;

public static class ObjectUtils
{
    /// <summary>
    /// Wrapper over FindObjectsOfType to allow searching by any type and with predicate.
    /// Be aware this is slow and scales lineary with scene complexity.
    /// </summary>
    public static T FindObject<T> (Predicate<T> predicate = null) where T : class
    {
        return Object.FindObjectsOfType<Object>().FirstOrDefault(obj => obj is T &&
            (predicate == null || predicate(obj as T))) as T;
    }

    /// <summary>
    /// Wrapper over FindObjectsOfType to allow searching by any type and with predicate.
    /// Be aware this is slow and scales lineary with scene complexity.
    /// </summary>
    public static List<T> FindObjects<T> (Predicate<T> predicate = null) where T : class
    {
        return Object.FindObjectsOfType<Object>().Where(obj => obj is T &&
            (predicate == null || predicate(obj as T))).Cast<T>().ToList();
    }

    /// <summary>
    /// Asserts there is only one instance of the object instantiated on scene.
    /// </summary>
    public static void AssertSingleInstance (this Object unityObject)
    {
        var objectType = unityObject.GetType();
        Debug.Assert(Object.FindObjectsOfType(objectType).Length == 1,
           string.Format("More than one instance of {0} found on scene.", objectType.Name));
    }
}

