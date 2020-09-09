using System.Collections;
using UnityCommon;
using UnityEngine;

public class TestApplicationBehaviour : MonoBehaviour
{
    private void Awake ()
    {
        DontDestroyOnLoad(gameObject);
        ApplicationBehaviour.Instance.StartCoroutine(Coroutine());
    }

    private IEnumerator Coroutine ()
    {
        while (Application.isPlaying)
        {
            yield return null;
            transform.localScale = Vector3.one * (Mathf.Sin(Time.time) + 2);
        }
    }
}
