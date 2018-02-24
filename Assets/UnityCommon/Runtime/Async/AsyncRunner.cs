using System;
using System.Collections;
using UnityEngine;

class AsyncRunnerContainer : MonoBehaviour { }

/// <summary>
/// Allows running custom asynchronous logic via coroutine.
/// </summary>
public abstract class AsyncRunner<TResult> : AsyncAction<TResult>
{
    public bool IsRunning { get { return coroutine != null; } }

    protected YieldInstruction YieldInstruction { get; set; }
    protected int CoroutineTickCount { get; private set; }

    private GameObject containerObject;
    private MonoBehaviour coroutineContainer;
    private IEnumerator coroutine;

    public AsyncRunner (MonoBehaviour coroutineContainer = null)
    {
        this.coroutineContainer = coroutineContainer ?? CreateContainer();
    }

    public AsyncRunner (MonoBehaviour coroutineContainer = null, Action onCompleted = null)
        : this (coroutineContainer)
    {
        if (onCompleted != null) OnCompleted += (_) => onCompleted.SafeInvoke();
    }

    public AsyncRunner (MonoBehaviour coroutineContainer = null, Action<TResult> onCompleted = null)
        : this(coroutineContainer)
    {
        if (onCompleted != null) OnCompleted += onCompleted;
    }

    public virtual AsyncRunner<TResult> Run ()
    {
        Stop();

        IsCompleted = false;

        if (!coroutineContainer || !coroutineContainer.gameObject || !coroutineContainer.gameObject.activeInHierarchy)
        {
            HandleOnCompleted();
            return this;
        }

        coroutine = AsyncRoutine();
        coroutineContainer.StartCoroutine(coroutine);

        return this;
    }

    public override void Reset ()
    {
        Stop();
        base.Reset();
    }

    public virtual void Stop ()
    {
        if (!IsRunning) return;

        if (coroutineContainer)
            coroutineContainer.StopCoroutine(coroutine);
        coroutine = null;
    }

    protected virtual bool LoopCondition ()
    {
        return CoroutineTickCount == 0;
    }

    protected virtual void OnCoroutineTick ()
    {
        CoroutineTickCount++;
    }

    public override void CompleteInstantly ()
    {
        Stop();
        base.CompleteInstantly();
    }

    public override void CompleteInstantly (TResult state)
    {
        Stop();
        base.CompleteInstantly(state);
    }

    protected override void HandleOnCompleted ()
    {
        if (containerObject)
            UnityEngine.Object.Destroy(containerObject);
        base.HandleOnCompleted();
    }

    protected virtual IEnumerator AsyncRoutine ()
    {
        while (LoopCondition())
        {
            OnCoroutineTick();
            yield return YieldInstruction;
        }

        HandleOnCompleted();
    }

    private MonoBehaviour CreateContainer ()
    {
        containerObject = new GameObject("AsyncRunnerContainer");
        containerObject.hideFlags = HideFlags.HideAndDontSave;
        return containerObject.AddComponent<AsyncRunnerContainer>();
    }
}
