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
    }
}
