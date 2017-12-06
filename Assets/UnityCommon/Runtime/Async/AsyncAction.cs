using System;
using UnityEngine;

/// <summary>
/// Represents an asynchronous operation. Can be used to suspend coroutines.
/// </summary>
public class AsyncAction : CustomYieldInstruction
{
    /// <summary>
    /// Event invoked when action has completed execution.
    /// </summary>
    public event Action OnCompleted;

    /// <summary>
    /// Whether action has completed execution.
    /// </summary>
    public virtual bool IsCompleted { get; protected set; }
    /// <summary>
    /// Whether action can instantly complete execution 
    /// and use <see cref="CompleteInstantly"/>.
    /// </summary>
    public virtual bool CanBeInstantlyCompleted { get { return true; } }
    public override bool keepWaiting { get { return !IsCompleted; } }

    /// <summary>
    /// Forces the action to complete instantly.
    /// Works only when <see cref="CanBeInstantlyCompleted"/>.
    /// </summary>
    public virtual void CompleteInstantly ()
    {
        if (!CanBeInstantlyCompleted || IsCompleted) return;
        HandleOnCompleted();
    }

    /// <summary>
    /// Adds a delegate to invoke when the action is completed execution.
    /// If the action is already completed, the delegate will be invoked immediately.
    /// </summary>
    public virtual void Then (Action action)
    {
        if (IsCompleted) action.SafeInvoke();
        else OnCompleted += action;
    }

    protected virtual void HandleOnCompleted ()
    {
        IsCompleted = true;
        OnCompleted.SafeInvoke();
    }
}

/// <summary>
/// Represents a <see cref="AsyncAction"/> with a strongly-typed state object.
/// </summary>
public class AsyncAction<TState> : AsyncAction 
{
    /// <summary>
    /// Event invoked when action has completed execution.
    /// </summary>
    public new event Action<TState> OnCompleted;

    /// <summary>
    /// Payload data describing action execution.
    /// </summary>
    public TState State { get; protected set; }

    public AsyncAction () : base() { }

    public AsyncAction (TState state) : base()
    {
        State = state;
    }

    /// <summary>
    /// Forces action to complete instantly; allows modifying <see cref="State"/> object.
    /// Works only when <see cref="CanBeInstantlyCompleted"/>.
    /// </summary>
    public virtual void CompleteInstantly (TState state)
    {
        State = state;
        base.CompleteInstantly();
    }

    /// <summary>
    /// Adds a delegate to invoke when the action is completed execution.
    /// If the action is already completed, the delegate will be invoked immediately.
    /// </summary>
    public virtual void Then (Action<TState> action)
    {
        if (IsCompleted) action.SafeInvoke(State);
        else OnCompleted += action;
    }

    protected override void HandleOnCompleted ()
    {
        base.HandleOnCompleted();
        OnCompleted.SafeInvoke(State);
    }
}
