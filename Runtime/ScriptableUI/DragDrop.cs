using UnityEngine;

namespace UnityCommon
{
    /// <summary>
    /// When attached to an uGUI object, implements drag-drop behaviour.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class DragDrop : MonoBehaviour
    {
        [SerializeField] private DragDropHandle handle = default;

        private RectTransform trs;
        private RectTransform handleTrs;
        private Canvas canvas;

        private void Awake ()
        {
            this.AssertRequiredObjects(handle);

            trs = GetComponent<RectTransform>();
            handleTrs = handle.GetComponent<RectTransform>();
            canvas = gameObject.FindTopmostComponent<Canvas>();
        }

        private void OnEnable ()
        {
            handle.OnDragged += HandleDrag;
        }

        private void OnDisable ()
        {
            handle.OnDragged -= HandleDrag;
        }

        private void HandleDrag (Vector2 position)
        {
            if (!canvas) return;

            position /= canvas.scaleFactor;

            var dragPos = trs.TransformPoint(position) - handleTrs.position;

            trs.position = new Vector3(dragPos.x, dragPos.y, trs.position.z);
        }
    }
}