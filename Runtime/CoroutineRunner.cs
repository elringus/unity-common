using System;
using System.Collections;
using System.Threading;
using UniRx.Async;
using UnityEngine;

namespace UnityCommon
{
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
        public virtual bool Completed => CompletionTask.IsCompleted;
        /// <summary>
        /// Whether the coroutine is currently running.
        /// </summary>
        public virtual bool Running => coroutine != null;
        /// <summary>
        /// Whether the coroutine can instantly complete execution and use <see cref="CompleteInstantly"/>.
        /// </summary>
        public virtual bool CanInstantlyComplete => true;
        public override bool keepWaiting => !Completed;

        protected YieldInstruction YieldInstruction { get; set; }
        protected int CoroutineTickCount { get; private set; }
        protected UniTask CompletionTask => completionSource.Task;
        protected CancellationToken CancellationToken { get; private set; }

        private readonly MonoBehaviour coroutineContainer;
        private UniTaskCompletionSource<CoroutineRunner> completionSource;
        private IEnumerator coroutine;

        public CoroutineRunner (MonoBehaviour coroutineContainer = null, YieldInstruction yieldInstruction = null)
        {
            completionSource = new UniTaskCompletionSource<CoroutineRunner>();
            this.coroutineContainer = ObjectUtils.IsValid(coroutineContainer) ? coroutineContainer : ApplicationBehaviour.Instance;
            YieldInstruction = yieldInstruction;
        }

        /// <summary>
        /// Starts the coroutine execution. 
        /// If the coroutine is already running or completed will <see cref="Reset"/> before running.
        /// </summary>
        public virtual void Run (CancellationToken cancellationToken = default)
        {
            if (Running || Completed) Reset();

            if (!coroutineContainer || !coroutineContainer.gameObject || !coroutineContainer.gameObject.activeInHierarchy)
            {
                HandleOnCompleted();
                return;
            }

            CancellationToken = cancellationToken;
            coroutine = CoroutineLoop();
            coroutineContainer.StartCoroutine(coroutine);
        }

        public virtual async UniTask RunAsync (CancellationToken cancellationToken = default)
        {
            Run(cancellationToken);
            await CompletionTask;
        }

        /// <summary>
        /// Stops (if running) and resets the coroutine state.
        /// </summary>
        public virtual new void Reset ()
        {
            Stop();
            completionSource = new UniTaskCompletionSource<CoroutineRunner>();
            base.Reset();
        }

        /// <summary>
        /// Halts the coroutine execution. Has no effect if the coroutine is not running.
        /// </summary>
        public virtual void Stop ()
        {
            if (!Running) return;

            if (coroutineContainer)
                coroutineContainer.StopCoroutine(coroutine);
            coroutine = null;
        }

        /// <summary>
        /// Forces the coroutine to complete instantly.
        /// Works only when <see cref="CanInstantlyComplete"/>.
        /// </summary>
        public virtual void CompleteInstantly ()
        {
            if (!CanInstantlyComplete || Completed) return;
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

        public UniTask<CoroutineRunner>.Awaiter GetAwaiter () => completionSource.Task.GetAwaiter();

        protected virtual void HandleOnCompleted ()
        {
            coroutine = null;
            completionSource.TrySetResult(this);
            OnCompleted?.Invoke();
        }

        protected virtual bool LoopCondition ()
        {
            return !CancellationToken.IsCancellationRequested && CoroutineTickCount == 0;
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
}
