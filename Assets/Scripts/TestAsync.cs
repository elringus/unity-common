using UnityCommon;
using UnityEngine;

public class TestAsync : MonoBehaviour
{
    public class EternalYeild : CustomYieldInstruction
    {
        public override bool keepWaiting => Application.isPlaying;
    }

    private readonly WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
    private readonly EternalYeild eternalYeild = new EternalYeild();

    private void Start ()
    {
        EndOfFrame();
    }

    private async void EndOfFrame ()
    {
        while (Application.isPlaying)
            await waitForEndOfFrame;
    }

    private async void CustomYeild ()
    {
        await eternalYeild;
    }
}
