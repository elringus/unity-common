using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UnityCommon
{
    public static class EventUtils
    {
        private static readonly List<RaycastResult> raycastResults = new List<RaycastResult>();

        /// <summary>
        /// Get top-most hovered game object.
        /// </summary>
        public static GameObject GetHoveredGameObject ()
        {
            var eventSystem = EventSystem.current;
            if (!eventSystem) throw new Error("Failed to get hovered object: event system is not available.");
            var data = new PointerEventData(eventSystem);
            #if ENABLE_LEGACY_INPUT_MANAGER
            data.position = Input.touchCount > 0 ? (Vector3)Input.GetTouch(0).position : Input.mousePosition;
            #elif ENABLE_INPUT_SYSTEM && INPUT_SYSTEM_AVAILABLE
            data.position = UnityEngine.InputSystem.Touchscreen.current?.touches.Count > 0
                ? UnityEngine.InputSystem.Touchscreen.current.touches[0].position.ReadValue()
                : UnityEngine.InputSystem.Mouse.current?.position.ReadValue() ?? Vector2.negativeInfinity;
            #endif
            raycastResults.Clear();
            eventSystem.RaycastAll(data, raycastResults);
            var topmost = default(RaycastResult?);
            foreach (var result in raycastResults)
                if (!topmost.HasValue || topmost.Value.distance < result.distance)
                    topmost = result;
            return topmost?.gameObject;
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

    [Serializable]
    public class StringUnityEvent : UnityEvent<string> { }

    [Serializable]
    public class FloatUnityEvent : UnityEvent<float> { }

    [Serializable]
    public class IntUnityEvent : UnityEvent<int> { }

    [Serializable]
    public class BoolUnityEvent : UnityEvent<bool> { }

    [Serializable]
    public class Vector3UnityEvent : UnityEvent<Vector3> { }

    [Serializable]
    public class Vector2UnityEvent : UnityEvent<Vector2> { }

    [Serializable]
    public class QuaternionUnityEvent : UnityEvent<Quaternion> { }

    [Serializable]
    public class ColorUnityEvent : UnityEvent<Color> { }
}
