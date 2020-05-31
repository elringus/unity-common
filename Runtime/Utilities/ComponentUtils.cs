using System.Collections.Generic;
using UnityEngine;

namespace UnityCommon
{
    public static class ComponentUtils
    {
        /// <summary>
        /// Unlike GetComponentsInChildren, does not include components on the caller object.
        /// </summary>
        public static List<T> GetComponentsOnChildren<T> (this GameObject gameObject) where T : Component
        {
            var result = new List<T>(gameObject.GetComponentsInChildren<T>());
            var compInCaller = result.Find(c => c.gameObject == gameObject);
            if (compInCaller) result.Remove(compInCaller);

            return result;
        }

        /// <inheritdoc cref="GetComponentsOnChildren{T}(GameObject)"/>
        public static List<T> GetComponentsOnChildren<T> (this Component component) where T : Component
        {
            return GetComponentsOnChildren<T>(component.gameObject);
        }

        /// <summary>
        /// Finds first topmost component of the provided type.
        /// </summary>
        public static T FindTopmostComponent<T> (this GameObject gameObject) where T : Component
        {
            var parentComps = gameObject.GetComponentsInParent<T>();
            if (parentComps != null && parentComps.Length > 0)
                return parentComps[parentComps.Length - 1];
            return null;
        }

        /// <inheritdoc cref="FindTopmostComponent{T}(GameObject)"/>
        public static T FindTopmostComponent<T> (this Component component) where T : Component
        {
            return FindTopmostComponent<T>(component.gameObject);
        }
    }
}
