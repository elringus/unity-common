using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    public Camera Camera { get { return _camera; } }
    public float ParallaxFactor { get { return _parallaxFactor; } }

    private Transform cameraTrs;
    private float initialOffset;

    [SerializeField] private Camera _camera = null;
    [Range(0f, 1f)]
    [SerializeField] private float _parallaxFactor = 1f;

    private void Awake ()
    {
        cameraTrs = Camera ? Camera.transform : Camera.main.transform;
        Debug.Assert(cameraTrs, "Assign required objects to ParallaxLayer.");
        initialOffset = (transform.position.x - cameraTrs.position.x) / ParallaxFactor;
    }

    private void Update ()
    {
        var targetPosX = cameraTrs.position.x + initialOffset;
        transform.SetPosX(targetPosX * ParallaxFactor);
    }
}
