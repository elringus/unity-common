using UnityCommon;
using UnityEngine;

public class TestAsync : MonoBehaviour
{
    public class EternalYeild : CustomYieldInstruction
    {
        public override bool keepWaiting => Application.isPlaying;
    }

    private readonly WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
    private readonly WaitWhile waitEvenTime = new WaitWhile(() => Time.time % 2 == 0);

    private void Start ()
    {
        EndOfFrame();
        CustomYeild();
    }

    private async void EndOfFrame ()
    {
        while (Application.isPlaying)
            await waitForEndOfFrame;
    }

    private async void CustomYeild ()
    {
        while (Application.isPlaying)
            await waitEvenTime;
    }
}
