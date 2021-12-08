using UnityEngine;

namespace UnityCommon
{
    /// <summary>
    /// When attached to an uGUI object, implements drag-drop behaviour.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class DragDrop : MonoBehaviour
    {
        [SerializeField] private DragDropHandle handle;
        [Tooltip("Whether to prevent dragging over the canvas bounds.")]
        [SerializeField] private bool clipOverCanvas = true;

        private RectTransform trs;
        private RectTransform handleTrs;
        private Canvas canvas;

        protected virtual void Awake ()
        {
            this.AssertRequiredObjects(handle);

            trs = GetComponent<RectTransform>();
            handleTrs = handle.GetComponent<RectTransform>();
            canvas = gameObject.FindTopmostComponent<Canvas>();
        }

        protected virtual void OnEnable ()
        {
            handle.OnDragged += HandleDrag;
        }

        protected virtual void OnDisable ()
        {
            handle.OnDragged -= HandleDrag;
        }

        protected virtual void HandleDrag (Vector2 position)
        {
            if (!canvas) return;

            if (clipOverCanvas && !canvas.pixelRect.Contains(position)) return;

            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay && ObjectUtils.IsValid(canvas.worldCamera))
                position -= canvas.pixelRect.size / 2;
            
            position /= canvas.scaleFactor;

            var dragPos = trs.TransformPoint(position) - handleTrs.position;

            trs.position = new Vector3(dragPos.x, dragPos.y, trs.position.z);
        }
    }
}
