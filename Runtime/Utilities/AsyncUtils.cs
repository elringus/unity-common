using UniRx.Async;

namespace UnityCommon
{
    public static class AsyncUtils
    {
        public static YieldAwaitable WaitEndOfFrame => UniTask.Yield();

        public static UniTask.Awaiter GetAwaiter (this UniTask? task) => task.HasValue ? task.GetAwaiter() : UniTask.CompletedTask.GetAwaiter();

        public static UniTask<T>.Awaiter GetAwaiter<T> (this UniTask<T>? task) => task.HasValue ? task.GetAwaiter() : UniTask.FromResult<T>(default).GetAwaiter();
    }
}
