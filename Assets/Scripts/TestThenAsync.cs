using UnityEngine;

public class TestThenAsync : MonoBehaviour
{
    private Tweener<VectorTween> tweener;
    private Timer timer;

    private void Awake ()
    {
        tweener = new Tweener<VectorTween>(this, () => print("Finished moving to: " + tweener.Result.TargetValue));
        timer = new Timer(coroutineContainer: this, onCompleted: () => print("Finished waiting for: " + timer.Duration));
    }

    private void OnEnable ()
    {
        //MoveTo(new Vector3(0, 0), .5f)
        //    .ThenAsync(() => MoveTo(new Vector3(1, 0), .5f))
        //    .ThenAsync(() => MoveTo(new Vector3(2, 0), .5f))
        //    .ThenAsync(() => MoveTo(new Vector3(3, 0), .5f));

        WaitFor(.1f)
            .ThenAsync(() => WaitFor(.2f))
            .ThenAsync(() => WaitFor(.3f))
            .ThenAsync(() => WaitFor(.4f));

        //new Timer().Run(.1f)
        //    .ThenAsync(() => new Timer().Run(.2f))
        //    .ThenAsync(() => new Timer().Run(.3f))
        //    .ThenAsync(() => new Timer().Run(.4f));
    }

    private AsyncAction MoveTo (Vector3 pos, float time)
    {
        print("Moving to: " + pos);
        var tween = new VectorTween(transform.position, pos, time, value => transform.position = value, false, true);
        return tweener.Run(tween);
    }

    private AsyncAction WaitFor (float time)
    {
        print("Waiting for: " + time);
        return timer.Run(time);
    }
}
