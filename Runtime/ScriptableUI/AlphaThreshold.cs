using UnityEngine;
using UnityEngine.UI;

namespace UnityCommon
{
    public class AlphaThreshold : MonoBehaviour
    {
        [SerializeField] private Image graphic;
        [SerializeField] private float minimumThreshold;

        private void Start ()
        {
            if (graphic) graphic.alphaHitTestMinimumThreshold = minimumThreshold;
        }
    }
}
