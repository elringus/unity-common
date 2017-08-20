using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public abstract class AsyncRunner
{
    class AsyncRunnerContainer : MonoBehaviour { }

    public readonly UnityEvent OnCompleteEvent = new UnityEvent();

    public abstract bool CanBeInstantlyCompleted { get; }
    public bool IsComplete { get; private set; }

    protected MonoBehaviour CoroutineContainer { get; private set; }
    protected AsyncOperation AsyncOperation { get; private set; }
    protected IEnumerator Coroutine { get; private set; }
    protected int RoutineTickCount { get; private set; }

    private GameObject containerObject;

    public AsyncRunner (MonoBehaviour coroutineContainer = null, UnityAction onComplete = null)
    {
        Context.Register(this);

        IsComplete = false;
        RoutineTickCount = 0;
        if (coroutineContainer) CoroutineContainer = coroutineContainer;
        else CoroutineContainer = CreateContainer();
        if (onComplete != null)
            OnCompleteEvent.AddListener(onComplete);
    }

    public virtual void Stop ()
    {
        if (Coroutine != null)
        {
            if (CoroutineContainer)
                CoroutineContainer.StopCoroutine(Coroutine);
            Coroutine = null;
        }
    }

    public virtual void CompleteInstantly ()
    {
        if (!CanBeInstantlyCompleted || IsComplete) return;

        Stop();
        OnComplete();
    }

    protected void RunAsyncOperation (AsyncOperation asyncOperation)
    {
        AsyncOperation = asyncOperation;
        StartCoroutine();
    }

    protected virtual void StartCoroutine ()
    {
        if (!CoroutineContainer)
        {
            Debug.LogWarning("Attempted to start coroutine in a runner with null container.");
            OnComplete();
            return;
        }

        Stop();

        if (!CoroutineContainer.gameObject.activeInHierarchy)
        {
            OnComplete();
            return;
        }

        Coroutine = AsyncRoutine();
        CoroutineContainer.StartCoroutine(Coroutine);
    }

    protected virtual bool LoopCondition ()
    {
        return RoutineTickCount == 0;
    }

    protected virtual void OnRoutineTick () { }

    protected virtual void OnComplete ()
    {
        IsComplete = true;
        OnCompleteEvent.Invoke();
        if (containerObject)
            UnityEngine.Object.Destroy(containerObject);
    }

    private IEnumerator AsyncRoutine ()
    {
        while (LoopCondition())
        {
            OnRoutineTick();
            yield return AsyncOperation;
            RoutineTickCount++;
        }

        OnComplete();
    }

    private MonoBehaviour CreateContainer ()
    {
        containerObject = new GameObject("AsyncRunnerContainer");
        containerObject.hideFlags = HideFlags.HideAndDontSave;
        return containerObject.AddComponent<AsyncRunnerContainer>();
    }
}

