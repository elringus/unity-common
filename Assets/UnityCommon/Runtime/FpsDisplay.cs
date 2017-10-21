using System.Collections;
using UnityEngine;

[RequireComponent(typeof(GUIText))]
public class FpsDisplay : MonoBehaviour
{
    [SerializeField] private float updateFrequency = 1f;

    private GUIText cachedText;

    private void Awake ()
    {
        cachedText = GetComponent<GUIText>();
    }

    private void Start ()
    {
        StartCoroutine(UpdateCounter());
    }

    private IEnumerator UpdateCounter ()
    {
        var waitForDelay = new WaitForSeconds(updateFrequency);

        while (true)
        {
            var lastFrameCount = Time.frameCount;
            var lastTime = Time.realtimeSinceStartup;

            yield return waitForDelay;

            var timeDelta = Time.realtimeSinceStartup - lastTime;
            var frameDelta = Time.frameCount - lastFrameCount;

            cachedText.text = string.Format("{0:0.} FPS", frameDelta / timeDelta);
        }
    }
}
