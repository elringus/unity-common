using UniRx.Async;
using UnityCommon;
using UnityEngine;

public class TestAsync : MonoBehaviour
{
    public class EternalYeild : CustomYieldInstruction
    {
        public override bool keepWaiting => Application.isPlaying;
    }

    private void Start ()
    {
        EndOfFrame();
        CustomYeild();
    }

    private async void EndOfFrame ()
    {
        while (Application.isPlaying)
            await AsyncUtils.WaitEndOfFrame;
    }

    private async void CustomYeild ()
    {
        while (Application.isPlaying)
            await UniTask.WaitWhile(() => Time.time % 2 == 0);
    }
}
