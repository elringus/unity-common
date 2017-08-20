using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    public Camera Camera;
    [Range(0f, 1f)]
    public float ParallaxFactor = 1f;

    private Transform cameraTrs;
    private float initialOffset;

    private void Awake ()
    {
        cameraTrs = Camera ? Camera.transform : Camera.main.transform;
        Debug.Assert(cameraTrs, "Assign a camera.");
        initialOffset = (transform.position.x - cameraTrs.position.x) / ParallaxFactor;
    }

    private void Update ()
    {
        var targetPosX = cameraTrs.position.x + initialOffset;
        transform.SetPosX(targetPosX * ParallaxFactor);
    }
}
