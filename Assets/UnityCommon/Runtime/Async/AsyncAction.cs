using System;
using UnityEngine;

/// <summary>
/// Represents an asynchronous operation. Can be used to suspend coroutines.
/// </summary>
public class AsyncAction : CustomYieldInstruction
{
    /// <summary>
    /// Event invoked when asynchronous operation has completed execution.
    /// </summary>
    public event Action OnCompleted;

    public virtual bool IsCompleted { get; protected set; }
    public virtual bool CanBeInstantlyCompleted { get { return true; } }
    public override bool keepWaiting { get { return !IsCompleted; } }

    public virtual void CompleteInstantly ()
    {
        if (!CanBeInstantlyCompleted || IsCompleted) return;
        HandleOnCompleted();
    }

    protected virtual void HandleOnCompleted ()
    {
        IsCompleted = true;
        OnCompleted.SafeInvoke();
    }
}

/// <summary>
/// Represents a <see cref="AsyncAction"/> with a strongly-typed result object.
/// </summary>
public class AsyncAction<TResult> : AsyncAction 
{
    public TResult Result { get; protected set; }

    public virtual void CompleteInstantly (TResult result)
    {
        Result = result;
        base.CompleteInstantly();
    }
}
