using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Object = UnityEngine.Object;

namespace UnityCommon
{
    public static class ObjectUtils
    {
        /// <summary>
        /// Invokes <see cref="Object.Destroy(Object)"/> or <see cref="Object.DestroyImmediate(Object)"/>
        /// depending on whether the application is in play mode. Won't have effect if the object is not valid.
        /// </summary>
        public static void DestroyOrImmediate (Object obj)
        {
            if (!IsValid(obj)) return;

            if (Application.isPlaying)
                Object.Destroy(obj);
            else Object.DestroyImmediate(obj);
        }

        /// <summary>
        /// Invokes <see cref="DestroyOrImmediate(Object)"/> on each direct descendent of the provided transform.
        /// </summary>
        public static void DestroyAllChilds (Transform trs)
        {
            var childCount = trs.childCount;
            for (int i = 0; i < childCount; i++)
                DestroyOrImmediate(trs.GetChild(i).gameObject);
        }

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

        /// <summary>
        /// Asserts validity of all the required objects.
        /// </summary>
        /// <param name="unityObject"></param>
        /// <param name="requiredObjects">Objects to check for validity.</param>
        /// <returns>Whether all the required objects are valid.</returns>
        public static bool AssertRequiredObjects (this Object unityObject, params Object[] requiredObjects)
        {
            var assertFailed = false;
            for (int i = 0; i < requiredObjects.Length; ++i)
            {
                if (!requiredObjects[i])
                {
                    Debug.LogError(string.Format("Required object of type '{0}' is not valid for '{1}'", requiredObjects[i]?.GetType()?.Name ?? "Unknown", unityObject.name));
                    assertFailed = true;
                }
            }
            return !assertFailed;
        }

        /// <summary>
        /// Invokes the provided action on each descendant (child of any level, recursively) and (optionally) on self.
        /// </summary>
        public static void ForEachDescendant (this GameObject gameObject, Action<GameObject> action, bool invokeOnSelf = true)
        {
            if (invokeOnSelf) action?.Invoke(gameObject);
            foreach (Transform childTransform in gameObject.transform)
                ForEachDescendant(childTransform.gameObject, action, true);
        }

        /// <summary>
        /// Checks if provided reference targets to a valid (not-destoyed) <see cref="UnityEngine.Object"/>.
        /// </summary>
        public static bool IsValid (object obj)
        {
            if (obj is UnityEngine.Object unityObject)
                return unityObject != null && unityObject;
            else return false;
        }
    }
}
