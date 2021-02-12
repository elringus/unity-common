using System.Threading.Tasks;
using UniRx.Async;
using UnityEngine;

public class TestAsync : MonoBehaviour
{
    private async void Start ()
    {
        while (Application.isPlaying)
            await TaskMethod();
    }

    private async Task TaskMethod ()
    {
        var startTime = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup - startTime < .1f)
            await Task.Yield();
    }

    private async UniTask UniTaskMethod ()
    {
        var startTime = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup - startTime < .1f)
            await UniTask.Yield();
    }
}
