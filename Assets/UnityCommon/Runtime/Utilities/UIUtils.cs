using UnityEngine;
using UnityEngine.UI;

namespace UnityCommon
{
    public static class UIUtils
    {
        /// <summary>
        /// Changes scrollbar's scroll position to contain specified child item.
        /// </summary>
        public static void ScrollTo (this ScrollRect scroller, RectTransform item)
        {
            var viewPos = (Vector2)scroller.transform.InverseTransformPoint(scroller.content.position);
            var itemPos = (Vector2)scroller.transform.InverseTransformPoint(item.position);
            var toPos = viewPos - itemPos - item.sizeDelta / 2f;
            if (!scroller.horizontal) toPos.x = scroller.content.anchoredPosition.x;
            if (!scroller.vertical) toPos.y = scroller.content.anchoredPosition.y;
            scroller.content.anchoredPosition = toPos;
        }

        /// <summary>
        /// Checks whether scrollbar contains both min and max points of the specified child item rect.
        /// </summary>
        public static bool Contains (this ScrollRect scroller, RectTransform item)
        {
            var min = scroller.viewport.InverseTransformPoint(item.TransformPoint(item.rect.min));
            var max = scroller.viewport.InverseTransformPoint(item.TransformPoint(item.rect.max));
            if (!scroller.horizontal) min.x = max.x = scroller.viewport.rect.center.x;
            if (!scroller.vertical) min.y = max.y = scroller.viewport.rect.center.y;
            return scroller.viewport.rect.Contains(min) && scroller.viewport.rect.Contains(max);
        }
    }
}
