using UnityEngine;

namespace UnityCommon
{
    public class Billboard : MonoBehaviour
    {
        private Transform cameraTransform;
        private Transform myTransform;

        [SerializeField] private bool disableWhenNotRendered = true;
        [Tooltip("Whether to apply a local z-offset to compensate root disposition with the parent transform.")]
        [SerializeField] private bool applyZOffset = false;
        [Tooltip("Used for the z-offset. Should be equal to the parent transform height, in units.")]
        [SerializeField] private float parentHeight = 1f;

        private void Awake ()
        {
            cameraTransform = Camera.main.transform;
            myTransform = transform;
        }

        private void Start ()
        {
            if (disableWhenNotRendered)
                enabled = false;
        }

        public void SetLookCamera (Camera camera)
        {
            cameraTransform = camera.transform;
        }

        private void LateUpdate ()
        {
            myTransform.forward = cameraTransform.forward;

            if (transform.parent && applyZOffset)
            {
                var zOffset = Mathf.Tan(transform.rotation.eulerAngles.x * Mathf.Deg2Rad) * parentHeight / 2f;
                transform.localPosition = new Vector3(0, 0, zOffset);
            }
        }

        private void OnBecameVisible ()
        {
            if (disableWhenNotRendered)
                enabled = true;
        }

        private void OnBecameInvisible ()
        {
            if (disableWhenNotRendered)
                enabled = false;
        }
    }
}
