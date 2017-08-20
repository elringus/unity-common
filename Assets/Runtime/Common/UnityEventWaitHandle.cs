using System;
using System.Collections.Generic;
using UnityEngine.Events;

public enum WaitFor
{
    AnyEvent,
    AllEvents
}

public class UnityEventWaitHandle : IDisposable
{
    public readonly UnityEvent OnComplete = new UnityEvent();

    public WaitFor WaitFor { get; private set; }
    public bool IsCompleted { get { return GetIsCompleted(); } }

    private List<Guid> completedEventIds;
    private int waitedEventCount = 0;
    private int completedEventCount = 0;
    private bool isReady = false;

    public UnityEventWaitHandle (WaitFor waitFor, UnityAction onComplete = null)
    {
        WaitFor = waitFor;
        if (onComplete != null)
            OnComplete.AddListener(onComplete);
        completedEventIds = new List<Guid>();
    }

    public static void WaitForAllAsyncRunners (UnityAction onComplete)
    {
        var pendingRunners = Context.ResolveAll<AsyncRunner>(runner => !runner.IsComplete);
        if (pendingRunners.Count == 0) { onComplete.SafeInvoke(); return; }

        using (var waiter = new UnityEventWaitHandle(WaitFor.AllEvents, onComplete))
            pendingRunners.ForEach(runner => runner.OnCompleteEvent.AddListener(waiter.Wait()));
    }

    public void Dispose ()
    {
        isReady = true;
        if (IsCompleted) Complete();
    }

    public UnityAction Wait ()
    {
        var waitedEventId = AddWaitedEvent();
        return () => OnWaitedEventComplete(waitedEventId);
    }

    public UnityAction<T0> Wait<T0> (T0 arg0)
    {
        var waitedEventId = AddWaitedEvent();
        return (_) => { OnWaitedEventComplete(waitedEventId); };
    }

    public UnityAction<T0, T1> Wait<T0, T1> (T0 arg0, T1 arg1)
    {
        var waitedEventId = AddWaitedEvent();
        return (_, __) => { OnWaitedEventComplete(waitedEventId); };
    }

    public UnityAction<T0, T1, T2> Wait<T0, T1, T2> (T0 arg0, T1 arg1, T2 arg2)
    {
        var waitedEventId = AddWaitedEvent();
        return (_, __, ___) => { OnWaitedEventComplete(waitedEventId); };
    }

    private Guid AddWaitedEvent ()
    {
        waitedEventCount++;
        return Guid.NewGuid();
    }

    private bool GetIsCompleted ()
    {
        if (!isReady) return false;

        switch (WaitFor)
        {
            case WaitFor.AnyEvent:
                return completedEventCount > 0;
            case WaitFor.AllEvents:
                return completedEventCount == waitedEventCount;
        }

        return false;
    }

    private void OnWaitedEventComplete (Guid waitedEventId)
    {
        if (completedEventIds.Contains(waitedEventId))
            return;
        completedEventIds.Add(waitedEventId);
        completedEventCount++;
        if (IsCompleted) Complete();
    }

    private void Complete ()
    {
        OnComplete.Invoke();
        OnComplete.RemoveAllListeners();
    }
}

