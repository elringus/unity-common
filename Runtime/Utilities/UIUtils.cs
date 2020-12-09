using UnityEngine;
using UnityEngine.UI;

namespace UnityCommon
{
    public static class UIUtils
    {
        public static void ScrollTo (this ScrollRect scroller, RectTransform child)
        {
            var contentPos = (Vector2)scroller.transform.InverseTransformPoint(scroller.content.position);
            var childPos = (Vector2)scroller.transform.InverseTransformPoint(child.position);
            var endPos = contentPos - childPos - child.sizeDelta / 2f;
            if (!scroller.horizontal) endPos.x = scroller.content.anchoredPosition.x;
            if (!scroller.vertical) endPos.y = scroller.content.anchoredPosition.y;
            scroller.content.anchoredPosition = endPos;
        }
    }
}
