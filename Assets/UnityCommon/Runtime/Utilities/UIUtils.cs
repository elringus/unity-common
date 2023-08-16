using UnityEngine;
using UnityEngine.UI;

namespace UnityCommon
{
    public static class UIUtils
    {
        /// <summary>
        /// Changes scrollbar's scroll position to contain specified child item.
        /// </summary>
        /// <param name="scroller"></param>
        /// <param name="child"></param>
        public static void ScrollTo (this ScrollRect scroller, RectTransform child)
        {
            var contentPos = (Vector2)scroller.transform.InverseTransformPoint(scroller.content.position);
            var childPos = (Vector2)scroller.transform.InverseTransformPoint(child.position);
            var endPos = contentPos - childPos - child.sizeDelta / 2f;
            if (!scroller.horizontal) endPos.x = scroller.content.anchoredPosition.x;
            if (!scroller.vertical) endPos.y = scroller.content.anchoredPosition.y;
            scroller.content.anchoredPosition = endPos;
        }

        /// <summary>
        /// Checks whether scrollbar contains both min and max points of the specified child item rect.
        /// </summary>
        public static bool Contains (this ScrollRect scroller, RectTransform child)
        {
            var min = scroller.viewport.InverseTransformPoint(child.TransformPoint(child.rect.min));
            var max = scroller.viewport.InverseTransformPoint(child.TransformPoint(child.rect.max));
            return scroller.viewport.rect.Contains(min) && scroller.viewport.rect.Contains(max);
        }
    }
}
