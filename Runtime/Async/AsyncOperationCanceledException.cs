using System;

namespace UnityCommon
{
    /// <summary>
    /// Thrown upon cancellation of an async operation via <see cref="AsyncToken"/>.
    /// </summary>
    public class AsyncOperationCanceledException : OperationCanceledException
    {
        public AsyncOperationCanceledException (AsyncToken asyncToken)
            : base(asyncToken.CancellationToken) { }
    }
}
