using UnityEngine;

public class TestAsyncSet : MonoBehaviour
{
    private void Start ()
    {
        var asyncSet = new AsyncActionSet();
        using (asyncSet)
        {
            asyncSet.AddAction(new Timer(0.50f).Run().Then(() => print("Timer 1 Complete")));
            asyncSet.AddAction(new Timer(0.55f).Run().Then(() => print("Timer 2 Complete")));
            asyncSet.AddAction(new Timer(0.85f).Run().Then(() => print("Timer 3 Complete")));
            asyncSet.AddAction(new Timer(1.00f).Run().Then(() => print("Timer 4 Complete")));
            asyncSet.AddAction(new Timer(1.25f).Run().Then(() => print("Timer 5 Complete")));
        }

        //var asyncSet = new AsyncActionSet(
        //        new Timer(0.50f).Run().Then(() => print("Timer 1 Complete")),
        //        new Timer(0.55f).Run().Then(() => print("Timer 2 Complete")),
        //        new Timer(0.85f).Run().Then(() => print("Timer 3 Complete")),
        //        new Timer(1.00f).Run().Then(() => print("Timer 4 Complete")),
        //        new Timer(1.25f).Run().Then(() => print("Timer 5 Complete"))
        //    );

        asyncSet.Then(() => print("Async Set Complete"));
    }

}
