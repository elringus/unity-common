using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Represents a set of <see cref="AsyncAction"/>.
/// <see cref="OnCompleted"/> event will be envoked when all tracked actions complete.
/// </summary>
public class AsyncActionSet : AsyncAction, IDisposable
{
    public float Progress { get { return completedActionCount / actions.Count; } }
    public bool IsReadyToComplete { get { return isAllActionsAdded && completedActionCount == actions.Count; } }
    public override bool CanBeInstantlyCompleted { get { return actions.All(a => a.CanBeInstantlyCompleted); } }

    private HashSet<AsyncAction> actions = new HashSet<AsyncAction>();
    private int completedActionCount;
    private bool isAllActionsAdded;

    public AsyncActionSet ()
    {
        isAllActionsAdded = false;
    }

    public AsyncActionSet (params AsyncAction[] asyncActions)
    {
        actions = new HashSet<AsyncAction>(asyncActions);
        isAllActionsAdded = true; 
        foreach (var action in actions)
            action.Then(HandleOnCompleted);
    }

    public void AddAction (AsyncAction action)
    {
        actions.Add(action);
    }

    public override void CompleteInstantly ()
    {
        if (!CanBeInstantlyCompleted || IsCompleted) return;

        foreach (var action in actions)
            action.CompleteInstantly();
    }

    public void Dispose ()
    {
        isAllActionsAdded = true;
        if (IsReadyToComplete)
            base.HandleOnCompleted();
    }

    protected override void HandleOnCompleted ()
    {
        completedActionCount++;
        if (IsReadyToComplete)
            base.HandleOnCompleted();
    }
}

/// <summary>
/// Represents a set of <see cref="AsyncAction{TState}"/>.
/// <see cref="OnCompleted"/> event will be envoked when all tracked actions complete.
/// </summary>
public class AsyncActionSet<TResult> : AsyncAction<TResult>, IDisposable
{
    public float Progress { get { return completedActionCount / actions.Count; } }
    public bool IsReadyToComplete { get { return isAllActionsAdded && completedActionCount == actions.Count; } }
    public override bool CanBeInstantlyCompleted { get { return actions.All(a => a.CanBeInstantlyCompleted); } }
    public new List<TResult> Result { get { return actions.Select(a => a.Result).ToList(); } }

    private HashSet<AsyncAction<TResult>> actions;
    private int completedActionCount;
    private bool isAllActionsAdded;

    public AsyncActionSet ()
    {
        isAllActionsAdded = false;
    }

    public AsyncActionSet (params AsyncAction<TResult>[] asyncActions)
    {
        actions = new HashSet<AsyncAction<TResult>>(asyncActions);
        isAllActionsAdded = true;
        foreach (var action in actions)
            action.Then(HandleOnCompleted);
    }

    public void AddAction (AsyncAction<TResult> action)
    {
        actions.Add(action);
    }

    public override void CompleteInstantly ()
    {
        if (!CanBeInstantlyCompleted || IsCompleted) return;

        foreach (var action in actions)
            action.CompleteInstantly();
    }

    public override void CompleteInstantly (TResult result)
    {
        if (!CanBeInstantlyCompleted || IsCompleted) return;

        foreach (var action in actions)
            action.CompleteInstantly(result);
    }

    public void Dispose ()
    {
        isAllActionsAdded = true;
        if (IsReadyToComplete)
            base.HandleOnCompleted();
    }

    protected override void HandleOnCompleted ()
    {
        completedActionCount++;
        if (IsReadyToComplete)
            base.HandleOnCompleted();
    }
}
