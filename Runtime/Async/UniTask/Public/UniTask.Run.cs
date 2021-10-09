using System;

namespace UnityCommon
{
    public readonly partial struct UniTask
    {
        /// <summary>Run action on the threadPool and return to main thread if configureAwait = true.</summary>
        public static async UniTask Run (Action action, bool configureAwait = true)
        {
            await SwitchToThreadPool();

            if (configureAwait)
            {
                try
                {
                    action();
                }
                finally
                {
                    await Yield();
                }
            }
            else
            {
                action();
            }
        }

        /// <summary>Run action on the threadPool and return to main thread if configureAwait = true.</summary>
        public static async UniTask Run (Action<object> action, object state, bool configureAwait = true)
        {
            await SwitchToThreadPool();

            if (configureAwait)
            {
                try
                {
                    action(state);
                }
                finally
                {
                    await Yield();
                }
            }
            else
            {
                action(state);
            }
        }

        /// <summary>Run action on the threadPool and return to main thread if configureAwait = true.</summary>
        public static async UniTask<T> Run<T> (Func<T> func, bool configureAwait = true)
        {
            await SwitchToThreadPool();
            if (configureAwait)
            {
                try
                {
                    return func();
                }
                finally
                {
                    await Yield();
                }
            }
            else
            {
                return func();
            }
        }

        /// <summary>Run action on the threadPool and return to main thread if configureAwait = true.</summary>
        public static async UniTask<T> Run<T> (Func<object, T> func, object state, bool configureAwait = true)
        {
            await SwitchToThreadPool();

            if (configureAwait)
            {
                try
                {
                    return func(state);
                }
                finally
                {
                    await Yield();
                }
            }
            else
            {
                return func(state);
            }
        }
    }
}
