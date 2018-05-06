using System.Collections;
using UnityEngine;

public class TestApplicationBehaviour : MonoBehaviour
{
    private void Awake ()
    {
        DontDestroyOnLoad(gameObject);
        ApplicationBehaviour.Singleton.StartCoroutine(Coroutine());
    }

    private IEnumerator Coroutine ()
    {
        while (true)
        {
            yield return null;
            transform.localScale = Vector3.one * (Mathf.Sin(Time.time) + 2);
        }
    }
}
