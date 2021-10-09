using System;
using System.Threading;

namespace UnityCommon.Async
{
    // UniTask has no scheduler like TaskScheduler.
    // Only handle unobserved exception.

    public static class UniTaskScheduler
    {
        public static event Action<Exception> UnobservedTaskException;

        /// <summary>
        /// Propagate OperationCanceledException to UnobservedTaskException when true. Default is false.
        /// </summary>
        public static bool PropagateOperationCanceledException = false;

        /// <summary>
        /// Write log type when catch unobserved exception and not registered UnobservedTaskException. Default is Warning.
        /// </summary>
        public static UnityEngine.LogType UnobservedExceptionWriteLogType = UnityEngine.LogType.Warning;

        /// <summary>
        /// Dispatch exception event to Unity MainThread.
        /// </summary>
        public static bool DispatchUnityMainThread = true;

        // cache delegate.
        private static readonly SendOrPostCallback handleExceptionInvoke = InvokeUnobservedTaskException;

        internal static void PublishUnobservedTaskException (Exception ex)
        {
            if (ex != null)
            {
                if (!PropagateOperationCanceledException && ex is OperationCanceledException)
                {
                    return;
                }

                if (UnobservedTaskException != null)
                {
                    if (Thread.CurrentThread.ManagedThreadId == PlayerLoopHelper.MainThreadId)
                    {
                        // allows inlining call.
                        UnobservedTaskException.Invoke(ex);
                    }
                    else
                    {
                        // Post to MainThread.
                        PlayerLoopHelper.UnitySynchronizationContext.Post(handleExceptionInvoke, ex);
                    }
                }
                else
                {
                    string msg = null;
                    if (UnobservedExceptionWriteLogType != UnityEngine.LogType.Exception)
                    {
                        msg = "UnobservedTaskException:" + ex.ToString();
                    }
                    switch (UnobservedExceptionWriteLogType)
                    {
                        case UnityEngine.LogType.Error:
                            UnityEngine.Debug.LogError(msg);
                            break;
                        case UnityEngine.LogType.Assert:
                            UnityEngine.Debug.LogAssertion(msg);
                            break;
                        case UnityEngine.LogType.Warning:
                            UnityEngine.Debug.LogWarning(msg);
                            break;
                        case UnityEngine.LogType.Log:
                            UnityEngine.Debug.Log(msg);
                            break;
                        case UnityEngine.LogType.Exception:
                            UnityEngine.Debug.LogException(ex);
                            break;
                    }
                }
            }
        }

        private static void InvokeUnobservedTaskException (object state)
        {
            UnobservedTaskException?.Invoke((Exception)state);
        }
    }
}
