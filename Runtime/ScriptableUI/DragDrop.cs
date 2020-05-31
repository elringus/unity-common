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

        private void Awake ()
        {
            this.AssertRequiredObjects(handle);

            trs = GetComponent<RectTransform>();
            handleTrs = handle.GetComponent<RectTransform>();
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
            trs.position = trs.TransformPoint(position) - handleTrs.position;
        }
    }
}