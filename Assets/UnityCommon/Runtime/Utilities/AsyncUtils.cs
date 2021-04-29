using UniRx.Async;

namespace UnityCommon
{
    public static class AsyncUtils
    {
        public static YieldAwaitable WaitEndOfFrame => UniTask.Yield(PlayerLoopTiming.PostLateUpdate);

        public static UniTask.Awaiter GetAwaiter (this UniTask? task) => task?.GetAwaiter() ?? UniTask.CompletedTask.GetAwaiter();

        public static UniTask<T>.Awaiter GetAwaiter<T> (this UniTask<T>? task) => task?.GetAwaiter() ?? UniTask.FromResult<T>(default).GetAwaiter();
    }
}
