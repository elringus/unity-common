using System.Collections;
using UnityCommon;
using UnityEngine;

public class TestTweener : MonoBehaviour
{
    private readonly Tweener<VectorTween> tweener = new Tweener<VectorTween>();

    private IEnumerator Start ()
    {
        while (Application.isPlaying)
        {
            var pos = transform.position;
            var tween = new VectorTween(pos, pos + Vector3.one, .1f, p => transform.position = p);
            tweener.Run(tween);
            while (tweener.Running) yield return null;
        }
    }
}
