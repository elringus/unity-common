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

        if (actions.Count == 0)
        {
            base.HandleOnCompleted();
            return;
        }

        foreach (var action in actions)
            action.Then(HandleOnCompleted);
    }

    public void AddAction (AsyncAction action)
    {
        if (isAllActionsAdded)
        {
            UnityEngine.Debug.LogError("Use default AsyncActionSet ctor to be able to add actions.");
            return;
        }

        action.Then(HandleOnCompleted);
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
public class AsyncActionSet<TResult> : AsyncAction<List<TResult>>, IDisposable
{
    public float Progress { get { return completedActionCount / actions.Count; } }
    public bool IsReadyToComplete { get { return isAllActionsAdded && completedActionCount == actions.Count; } }
    public override bool CanBeInstantlyCompleted { get { return actions.All(a => a.CanBeInstantlyCompleted); } }
    public override List<TResult> Result { get { return actions.Select(a => a.Result).ToList(); } }

    private List<AsyncAction<TResult>> actions;
    private int completedActionCount;
    private bool isAllActionsAdded;

    public AsyncActionSet ()
    {
        isAllActionsAdded = false;
    }

    public AsyncActionSet (params AsyncAction<TResult>[] asyncActions)
    {
        actions = new List<AsyncAction<TResult>>(asyncActions);
        isAllActionsAdded = true;
        foreach (var action in actions)
            action.Then(HandleOnCompleted);
    }

    public void AddAction (AsyncAction<TResult> action)
    {
        if (isAllActionsAdded)
        {
            UnityEngine.Debug.LogError("Use default AsyncActionSet ctor to be able to add actions.");
            return;
        }

        action.Then(HandleOnCompleted);
        actions.Add(action);
    }

    public override void CompleteInstantly ()
    {
        if (!CanBeInstantlyCompleted || IsCompleted) return;

        foreach (var action in actions)
            action.CompleteInstantly();
    }

    public override void CompleteInstantly (List<TResult> result)
    {
        if (!CanBeInstantlyCompleted || IsCompleted) return;

        for (int i = 0; i < result.Count; i++)
            actions[i].CompleteInstantly(result[i]);
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
