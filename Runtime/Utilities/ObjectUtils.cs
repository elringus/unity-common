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
        public static void DestroyAllChildren (Transform trs)
        {
            var childCount = trs.childCount;
            for (var i = 0; i < childCount; i++)
                DestroyOrImmediate(trs.GetChild(i).gameObject);
        }

        /// <summary>
        /// Wrapper over FindObjectsOfType to allow searching by any type and with predicate.
        /// Be aware this is slow and scales linearly with scene complexity.
        /// </summary>
        public static T FindObject<T> (Predicate<T> predicate = null) where T : class
        {
            return Object.FindObjectsOfType<Object>().FirstOrDefault(obj => obj is T arg &&
                                                                            (predicate == null || predicate(arg))) as T;
        }

        /// <summary>
        /// Wrapper over FindObjectsOfType to allow searching by any type and with predicate.
        /// Be aware this is slow and scales linearly with scene complexity.
        /// </summary>
        public static List<T> FindObjects<T> (Predicate<T> predicate = null) where T : class
        {
            return Object.FindObjectsOfType<Object>().Where(obj => obj is T arg &&
                                                                   (predicate == null || predicate(arg))).Cast<T>().ToList();
        }

        /// <summary>
        /// Asserts there is only one instance of the object instantiated on scene.
        /// </summary>
        public static void AssertSingleInstance (this Object unityObject)
        {
            var objectType = unityObject.GetType();
            Debug.Assert(Object.FindObjectsOfType(objectType).Length == 1,
                $"More than one instance of {objectType.Name} found on scene.");
        }

        /// <summary>
        /// Asserts validity of all the required objects.
        /// </summary>
        /// <param name="requiredObjects">Objects to check for validity.</param>
        /// <returns>Whether all the required objects are valid.</returns>
        public static void AssertRequiredObjects (this Component component, params Object[] requiredObjects)
        {
            if (requiredObjects.Any(obj => !obj))
                throw new UnityException($"Unity object `{component}` is missing a required dependency. " +
                                         "Make sure all the required fields are assigned in the inspector and are pointing to valid objects.");
        }

        /// <summary>
        /// Invokes the provided action on each descendant (child of any level, recursively) and (optionally) on self.
        /// </summary>
        public static void ForEachDescendant (this GameObject gameObject, Action<GameObject> action, bool invokeOnSelf = true)
        {
            if (invokeOnSelf) action?.Invoke(gameObject);
            foreach (Transform childTransform in gameObject.transform)
                ForEachDescendant(childTransform.gameObject, action);
        }

        /// <summary>
        /// Checks if provided reference targets to a valid (not-destroyed) <see cref="UnityEngine.Object"/>.
        /// </summary>
        public static bool IsValid (object obj)
        {
            if (obj is Object unityObject)
                return unityObject != null && unityObject;
            var d = new SerializableLiteralStringMap();
            d = new[] { ("", "") };
            return false;
        }

        /// <summary>
        /// Checks whether the provided game object is currently edited in prefab isolation mode.
        /// Always returns false in case provided object is not valid and in builds.
        /// </summary>
        public static bool IsEditedInPrefabMode (GameObject obj)
        {
            if (!IsValid(obj)) return false;
            #if UNITY_EDITOR
            #if UNITY_2021_2_OR_NEWER
            return UnityEditor.SceneManagement.PrefabStageUtility.GetPrefabStage(obj) != null;
            #else
            return UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetPrefabStage(obj) != null;
            #endif
            #else
            return false;
            #endif
        }

        /// <summary>
        /// When in editor and specified object is a valid project asset,
        /// returns asset's path formatted as hyperlink; otherwise, returns null.
        /// </summary>
        public static string BuildAssetLink (Object asset, int? line = null)
        {
            #if UNITY_EDITOR
            if (!asset) return null;
            return StringUtils.BuildAssetLink(UnityEditor.AssetDatabase.GetAssetPath(asset), line);
            #else
            return null;
            #endif
        }
    }
}
