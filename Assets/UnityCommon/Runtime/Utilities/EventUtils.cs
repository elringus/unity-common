using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityCommon
{
    public static class EventUtils
    {
        /// <summary>
        /// Get top-most hovered game object.
        /// </summary>
        public static GameObject GetHoveredGameObject (this EventSystem eventSystem)
        {
            #if ENABLE_LEGACY_INPUT_MANAGER
            var pointerEventData = new PointerEventData(EventSystem.current);
            pointerEventData.position = Input.touchCount > 0 ? (Vector3)Input.GetTouch(0).position : Input.mousePosition;

            var raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerEventData, raycastResults);
            if (raycastResults.Count > 0)
                return raycastResults[0].gameObject;
            else return null;
            #else
            Debug.LogWarning("`UnityCommon.GetHoveredGameObject` requires legacy input system, which is disabled; the method will always return null.");
            return null;
            #endif
        }

        public static void SafeInvoke (this Action action)
        {
            action?.Invoke();
        }

        public static void SafeInvoke<T0> (this Action<T0> action, T0 arg0)
        {
            action?.Invoke(arg0);
        }

        public static void SafeInvoke<T0, T1> (this Action<T0, T1> action, T0 arg0, T1 arg1)
        {
            action?.Invoke(arg0, arg1);
        }

        public static void SafeInvoke<T0, T1, T2> (this Action<T0, T1, T2> action, T0 arg0, T1 arg1, T2 arg2)
        {
            action?.Invoke(arg0, arg1, arg2);
        }
    }
}
