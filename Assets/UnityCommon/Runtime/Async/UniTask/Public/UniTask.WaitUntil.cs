using System;
using System.Collections.Generic;
using System.Threading;
using UnityCommon.Async.Internal;

namespace UnityCommon
{
    public readonly partial struct UniTask
    {
        public static UniTask WaitUntil (Func<bool> predicate, PlayerLoopTiming timing = PlayerLoopTiming.Update, CancellationToken cancellationToken = default)
        {
            var promise = new WaitUntilPromise(predicate, timing, cancellationToken);
            return promise.Task;
        }

        public static UniTask WaitWhile (Func<bool> predicate, PlayerLoopTiming timing = PlayerLoopTiming.Update, CancellationToken cancellationToken = default)
        {
            var promise = new WaitWhilePromise(predicate, timing, cancellationToken);
            return promise.Task;
        }

        public static UniTask<U> WaitUntilValueChanged<T, U> (T target, Func<T, U> monitorFunction, PlayerLoopTiming monitorTiming = PlayerLoopTiming.Update, IEqualityComparer<U> equalityComparer = null, CancellationToken cancellationToken = default) where T : class
        {
            var isUnityObject = !ReferenceEquals(target, null); // don't use (unityObject == null)

            return isUnityObject
                ? new WaitUntilValueChangedUnityObjectPromise<T, U>(target, monitorFunction, equalityComparer, monitorTiming, cancellationToken).Task
                // ReSharper disable once ExpressionIsAlwaysNull
                : new WaitUntilValueChangedStandardObjectPromise<T, U>(target, monitorFunction, equalityComparer, monitorTiming, cancellationToken).Task;
        }

        private class WaitUntilPromise : PlayerLoopReusablePromiseBase
        {
            private readonly Func<bool> predicate;

            public WaitUntilPromise (Func<bool> predicate, PlayerLoopTiming timing, CancellationToken cancellationToken)
                : base(timing, cancellationToken, 1)
            {
                this.predicate = predicate;
            }

            protected override void OnRunningStart () { }

            public override bool MoveNext ()
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Complete();
                    TrySetCanceled();
                    return false;
                }

                bool result = default(bool);
                try
                {
                    result = predicate();
                }
                catch (Exception ex)
                {
                    Complete();
                    TrySetException(ex);
                    return false;
                }

                if (result)
                {
                    Complete();
                    TrySetResult();
                    return false;
                }

                return true;
            }
        }

        private class WaitWhilePromise : PlayerLoopReusablePromiseBase
        {
            private readonly Func<bool> predicate;

            public WaitWhilePromise (Func<bool> predicate, PlayerLoopTiming timing, CancellationToken cancellationToken)
                : base(timing, cancellationToken, 1)
            {
                this.predicate = predicate;
            }

            protected override void OnRunningStart () { }

            public override bool MoveNext ()
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Complete();
                    TrySetCanceled();
                    return false;
                }

                bool result = default(bool);
                try
                {
                    result = predicate();
                }
                catch (Exception ex)
                {
                    Complete();
                    TrySetException(ex);
                    return false;
                }

                if (!result)
                {
                    Complete();
                    TrySetResult();
                    return false;
                }

                return true;
            }
        }

        // where T : UnityEngine.Object, can not add constraint
        private class WaitUntilValueChangedUnityObjectPromise<T, U> : PlayerLoopReusablePromiseBase<U>
        {
            private readonly T target;
            private readonly Func<T, U> monitorFunction;
            private readonly IEqualityComparer<U> equalityComparer;
            private U currentValue;

            public WaitUntilValueChangedUnityObjectPromise (T target, Func<T, U> monitorFunction, IEqualityComparer<U> equalityComparer, PlayerLoopTiming timing, CancellationToken cancellationToken)
                : base(timing, cancellationToken, 1)
            {
                this.target = target;
                this.monitorFunction = monitorFunction;
                this.equalityComparer = equalityComparer ?? UnityEqualityComparer.GetDefault<U>();
                this.currentValue = monitorFunction(target);
            }

            protected override void OnRunningStart () { }

            public override bool MoveNext ()
            {
                if (cancellationToken.IsCancellationRequested || target == null) // destroyed = cancel.
                {
                    Complete();
                    TrySetCanceled();
                    return false;
                }

                var nextValue = default(U);
                try
                {
                    nextValue = monitorFunction(target);
                    if (equalityComparer.Equals(currentValue, nextValue))
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Complete();
                    TrySetException(ex);
                    return false;
                }

                Complete();
                currentValue = nextValue;
                TrySetResult(nextValue);
                return false;
            }
        }

        private class WaitUntilValueChangedStandardObjectPromise<T, U> : PlayerLoopReusablePromiseBase<U>
            where T : class
        {
            private readonly WeakReference<T> target;
            private readonly Func<T, U> monitorFunction;
            private readonly IEqualityComparer<U> equalityComparer;
            private U currentValue;

            public WaitUntilValueChangedStandardObjectPromise (T target, Func<T, U> monitorFunction, IEqualityComparer<U> equalityComparer, PlayerLoopTiming timing, CancellationToken cancellationToken)
                : base(timing, cancellationToken, 1)
            {
                this.target = new WeakReference<T>(target, false); // wrap in WeakReference.
                this.monitorFunction = monitorFunction;
                this.equalityComparer = equalityComparer ?? UnityEqualityComparer.GetDefault<U>();
                this.currentValue = monitorFunction(target);
            }

            protected override void OnRunningStart () { }

            public override bool MoveNext ()
            {
                if (cancellationToken.IsCancellationRequested || !target.TryGetTarget(out var t))
                {
                    Complete();
                    TrySetCanceled();
                    return false;
                }

                var nextValue = default(U);
                try
                {
                    nextValue = monitorFunction(t);
                    if (equalityComparer.Equals(currentValue, nextValue))
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Complete();
                    TrySetException(ex);
                    return false;
                }

                Complete();
                currentValue = nextValue;
                TrySetResult(nextValue);
                return false;
            }
        }
    }
}
