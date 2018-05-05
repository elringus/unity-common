using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Allows running custom asynchronous logic via coroutine.
/// </summary>
public class CoroutineRunner : CustomYieldInstruction
{
    /// <summary>
    /// Event invoked when the coroutine has completed execution.
    /// </summary>
    public event Action OnCompleted;

    /// <summary>
    /// Whether the coroutine has completed execution.
    /// </summary>
    public virtual bool IsCompleted { get; protected set; }
    /// <summary>
    /// Whether the coroutine is currently running.
    /// </summary>
    public virtual bool IsRunning { get { return coroutine != null; } }
    /// <summary>
    /// Whether the coroutine can instantly complete execution and use <see cref="CompleteInstantly"/>.
    /// </summary>
    public virtual bool CanBeInstantlyCompleted { get { return true; } }
    public override bool keepWaiting { get { return !IsCompleted; } }

    protected YieldInstruction YieldInstruction { get; set; }
    protected int CoroutineTickCount { get; private set; }

    private MonoBehaviour coroutineContainer;
    private IEnumerator coroutine;

    public CoroutineRunner (MonoBehaviour coroutineContainer = null, YieldInstruction yieldInstruction = null)
    {
        this.coroutineContainer = coroutineContainer ?? ApplicationBehaviour.Singleton;
        YieldInstruction = yieldInstruction;
    }

    /// <summary>
    /// Starts the coroutine execution. 
    /// If the coroutine is already running will <see cref="Reset"/> before running.
    /// </summary>
    public virtual void Run ()
    {
        if (IsRunning) Reset();

        IsCompleted = false;

        if (!coroutineContainer || !coroutineContainer.gameObject || !coroutineContainer.gameObject.activeInHierarchy)
        {
            HandleOnCompleted();
            return;
        }

        coroutine = CoroutineLoop();
        coroutineContainer.StartCoroutine(coroutine);
    }

    /// <summary>
    /// Starts the coroutine execution using async model. 
    /// If the coroutine is already running will <see cref="Reset"/> before running.
    /// </summary>
    public virtual async Task RunAsync ()
    {
        Run();
        var taskCompletionSource = new TaskCompletionSource<object>();
        if (IsCompleted) taskCompletionSource.SetResult(null);
        else OnCompleted += () => taskCompletionSource.SetResult(null);
        await taskCompletionSource.Task;
    }

    /// <summary>
    /// Stops (if running) and resets the coroutine state.
    /// </summary>
    public virtual new void Reset ()
    {
        Stop();
        IsCompleted = false;
        base.Reset();
    }

    /// <summary>
    /// Halts the coroutine execution. Has no effect if the coroutine is not running.
    /// </summary>
    public virtual void Stop ()
    {
        if (!IsRunning) return;

        if (coroutineContainer)
            coroutineContainer.StopCoroutine(coroutine);
        coroutine = null;
    }

    /// <summary>
    /// Forces the coroutine to complete instantly.
    /// Works only when <see cref="CanBeInstantlyCompleted"/>.
    /// </summary>
    public virtual void CompleteInstantly ()
    {
        if (!CanBeInstantlyCompleted || IsCompleted) return;
        Stop();
        HandleOnCompleted();
    }

    /// <summary>
    /// Clears <see cref="OnCompleted"/> event invocation list.
    /// </summary>
    public virtual void RemoveAllOnCompleteListeners ()
    {
        OnCompleted = null;
    }

    public TaskAwaiter GetAwaiter ()
    {
        var taskCompletionSource = new TaskCompletionSource<object>();
        if (IsCompleted) taskCompletionSource.SetResult(null);
        else OnCompleted += () => taskCompletionSource.SetResult(null);
        return (taskCompletionSource.Task as Task).GetAwaiter();
    }

    protected virtual void HandleOnCompleted ()
    {
        IsCompleted = true;
        OnCompleted?.Invoke();
    }

    protected virtual bool LoopCondition ()
    {
        return CoroutineTickCount == 0;
    }

    protected virtual void OnCoroutineTick ()
    {
        CoroutineTickCount++;
    }

    protected virtual IEnumerator CoroutineLoop ()
    {
        while (LoopCondition())
        {
            OnCoroutineTick();
            yield return YieldInstruction;
        }

        HandleOnCompleted();
    }
}
