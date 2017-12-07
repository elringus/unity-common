using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Allows running custom asynchronous logic via coroutine.
/// </summary>
public abstract class AsyncRunner : AsyncAction
{
    class AsyncRunnerContainer : MonoBehaviour { }

    public bool IsRunning { get { return coroutine != null; } }

    protected YieldInstruction YieldInstruction { get; set; }
    protected int CoroutineTickCount { get; private set; }

    private GameObject containerObject;
    private MonoBehaviour coroutineContainer;
    private IEnumerator coroutine;

    public AsyncRunner (MonoBehaviour coroutineContainer = null, Action onCompleted = null)
    {
        this.coroutineContainer = coroutineContainer ?? CreateContainer();
        if (onCompleted != null)
            OnCompleted += onCompleted;
    }

    public virtual void Run ()
    {
        Stop();

        if (!coroutineContainer.gameObject.activeInHierarchy)
        {
            HandleOnCompleted();
            return;
        }

        coroutine = AsyncRoutine();
        coroutineContainer.StartCoroutine(coroutine);
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
