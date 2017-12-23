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

    public AsyncAction () : base() { }

    public AsyncAction (bool isInitiallyCompleted = false) : base()
    {
        IsCompleted = isInitiallyCompleted;
    }

    /// <summary>
    /// Returns new instance of <see cref="AsyncAction"/> with <see cref="IsCompleted"/> set to true.
    /// </summary>
    public static AsyncAction CreateCompleted ()
    {
        return new AsyncAction(true);
    }

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
    /// Adds a delegate to invoke when the action has completed execution.
    /// If the action is already completed, the delegate will be invoked immediately.
    /// </summary>
    public virtual AsyncAction Then (Action action)
    {
        if (IsCompleted) action.Invoke();
        else OnCompleted += action;

        return this;
    }

    /// <summary>
    /// Adds an async function to invoke when the action has completed execution.
    /// If the action is already completed, the function will be invoked immediately.
    /// Returned <see cref="AsyncAction"/> will complete after async function execution.
    /// </summary>
    public virtual AsyncAction ThenAsync (Func<AsyncAction> func)
    {
        if (IsCompleted) return func.Invoke();
        else
        {
            var promise = new AsyncAction();
            OnCompleted += () => func.Invoke().Then(promise.CompleteInstantly);
            return promise;
        }
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
    /// <summary>
    /// Event invoked when action has completed execution.
    /// </summary>
    public new event Action<TResult> OnCompleted;

    /// <summary>
    /// Object representing the result of the action execution.
    /// </summary>
    public TResult Result { get; protected set; }

    public AsyncAction () : base() { }

    public AsyncAction (TResult result, bool isInitiallyCompleted = false) 
        : base(isInitiallyCompleted)
    {
        Result = result;
    }

    /// <summary>
    /// Returns new instance of <see cref="AsyncAction{TResult}"/> with <see cref="IsCompleted"/> set to true.
    /// </summary>
    public static AsyncAction<TResult> CreateCompleted (TResult result)
    {
        return new AsyncAction<TResult>(result, true);
    }

    /// <summary>
    /// Forces action to complete instantly; allows modifying <see cref="Result"/> object.
    /// Works only when <see cref="CanBeInstantlyCompleted"/>.
    /// </summary>
    public virtual void CompleteInstantly (TResult result)
    {
        Result = result;
        base.CompleteInstantly();
    }

    /// <summary>
    /// Adds a delegate to invoke when the action has completed execution.
    /// If the action is already completed, the delegate will be invoked immediately.
    /// </summary>
    public virtual AsyncAction<TResult> Then (Action<TResult> action)
    {
        if (IsCompleted) action.Invoke(Result);
        else OnCompleted += action;

        return this;
    }

    /// <summary>
    /// Adds an async function to invoke when the action has completed execution.
    /// If the action is already completed, the function will be invoked immediately.
    /// Returned <see cref="AsyncAction"/> will complete after async function completion.
    /// </summary>
    public virtual AsyncAction ThenAsync (Func<TResult, AsyncAction> func)
    {
        if (IsCompleted) return func.Invoke(Result);
        else
        {
            var promise = new AsyncAction();
            OnCompleted += (result) => func.Invoke(result).Then(promise.CompleteInstantly);
            return promise;
        }
    }

    /// <summary>
    /// Adds an async function to invoke when the action has completed execution.
    /// If the action is already completed, the function will be invoked immediately.
    /// Returned <see cref="AsyncAction{TFunc}"/> will complete after async function completion.
    /// </summary>
    public virtual AsyncAction<TFunc> ThenAsync<TFunc> (Func<TResult, AsyncAction<TFunc>> func)
    {
        if (IsCompleted) return func.Invoke(Result);
        else
        {
            var promise = new AsyncAction<TFunc>();
            OnCompleted += (result) => func.Invoke(result).Then(promise.CompleteInstantly);
            return promise;
        }
    }

    protected override void HandleOnCompleted ()
    {
        base.HandleOnCompleted();
        OnCompleted.SafeInvoke(Result);
    }
}
