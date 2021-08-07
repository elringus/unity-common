using UniRx.Async;

namespace UnityCommon
{
    public static class AsyncUtils
    {
        public static UniTask.Awaiter GetAwaiter (this UniTask? task)
        {
            return task?.GetAwaiter() ?? UniTask.CompletedTask.GetAwaiter();
        }

        public static UniTask<T>.Awaiter GetAwaiter<T> (this UniTask<T>? task)
        {
            return task?.GetAwaiter() ?? UniTask.FromResult<T>(default).GetAwaiter();
        }

        public static async UniTask WaitEndOfFrameAsync (AsyncToken asyncToken = default)
        {
            await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
            asyncToken.ThrowIfCanceled();
        }

        public static async UniTask DelayFrameAsync (int frameCount, AsyncToken asyncToken = default)
        {
            await UniTask.DelayFrame(frameCount);
            asyncToken.ThrowIfCanceled();
        }
    }
}
