using System;
using System.Collections.Generic;

public class ActionWaitHandle : IDisposable
{
    public enum WaitForEnum { AnyAction, AllActions }

    public event Action OnComplete;

    public WaitForEnum WaitFor { get; private set; }
    public bool IsCompleted { get { return GetIsCompleted(); } }

    private List<Guid> completedActionIds;
    private int waitedActionsCount = 0;
    private int completedActionsCount = 0;
    private bool isReady = false;

    public ActionWaitHandle (WaitForEnum waitFor, Action onComplete = null)
    {
        WaitFor = waitFor;
        if (onComplete != null)
            OnComplete += onComplete;
        completedActionIds = new List<Guid>();
    }

    public static void WaitForAllAsyncRunners (Action onComplete)
    {
        var pendingRunners = Context.ResolveAll<AsyncRunner>(runner => !runner.IsCompleted);
        if (pendingRunners.Count == 0) { onComplete.SafeInvoke(); return; }

        using (var waiter = new ActionWaitHandle(WaitForEnum.AllActions, onComplete))
            pendingRunners.ForEach(runner => runner.OnCompleted += waiter.Wait());
    }

    public void Dispose ()
    {
        isReady = true;
        if (IsCompleted) Complete();
    }

    public Action Wait ()
    {
        var waitedEventId = AddWaitedAction();
        return () => OnWaitedActionComplete(waitedEventId);
    }

    public Action<T0> Wait<T0> (T0 arg0)
    {
        var waitedEventId = AddWaitedAction();
        return (_) => { OnWaitedActionComplete(waitedEventId); };
    }

    public Action<T0, T1> Wait<T0, T1> (T0 arg0, T1 arg1)
    {
        var waitedEventId = AddWaitedAction();
        return (_, __) => { OnWaitedActionComplete(waitedEventId); };
    }

    public Action<T0, T1, T2> Wait<T0, T1, T2> (T0 arg0, T1 arg1, T2 arg2)
    {
        var waitedEventId = AddWaitedAction();
        return (_, __, ___) => { OnWaitedActionComplete(waitedEventId); };
    }

    private Guid AddWaitedAction ()
    {
        waitedActionsCount++;
        return Guid.NewGuid();
    }

    private bool GetIsCompleted ()
    {
        if (!isReady) return false;

        switch (WaitFor)
        {
            case WaitForEnum.AnyAction:
                return completedActionsCount > 0;
            case WaitForEnum.AllActions:
                return completedActionsCount == waitedActionsCount;
        }

        return false;
    }

    private void OnWaitedActionComplete (Guid waitedActionId)
    {
        if (completedActionIds.Contains(waitedActionId))
            return;
        completedActionIds.Add(waitedActionId);
        completedActionsCount++;
        if (IsCompleted) Complete();
    }

    private void Complete ()
    {
        OnComplete.SafeInvoke();
        OnComplete = null;
    }
}
