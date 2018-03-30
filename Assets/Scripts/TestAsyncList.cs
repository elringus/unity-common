using System.Collections.Generic;
using UnityEngine;

public class TestAsyncList : MonoBehaviour
{
    class TimerObject
    {
        float order;
        float waitTime;

        public TimerObject (float order, float waitTime)
        {
            this.order = order;
            this.waitTime = waitTime;
        }

        public AsyncAction RunTimer ()
        {
            return new Timer(waitTime, onCompleted: () => print(string.Format("Order: {0} Waited for: {1}", order, waitTime))).Run();
        }
    }

    private void OnEnable ()
    {
        new List<TimerObject> {
            new TimerObject(0, 1.5f),
            new TimerObject(1, 0.1f),
            new TimerObject(2, 1),
            new TimerObject(3, 0),
            new TimerObject(4, 1.1f),
            new TimerObject(5, 1),
            new TimerObject(6, 0.5f),
        }.InvokeAsyncList(o => o.RunTimer());
    }
}
