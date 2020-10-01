using System;
using System.Collections.Generic;
using UniRx.Async;

namespace UnityCommon
{
    public abstract class ResourceRunner
    {
        public readonly string Path;
        public readonly Type ResourceType;

        protected ResourceRunner (string path, Type resourceType)
        {
            Path = path;
            ResourceType = resourceType;
        }

        public UniTask.Awaiter GetAwaiter () => GetAwaiterImpl();

        public abstract UniTask RunAsync ();
        public abstract void Cancel ();

        protected abstract UniTask.Awaiter GetAwaiterImpl ();
    }

    public abstract class ResourceRunner<TResult> : ResourceRunner
    {
        public TResult Result { get; private set; }

        private UniTaskCompletionSource<TResult> completionSource = new UniTaskCompletionSource<TResult>();

        protected ResourceRunner (string path, Type resourceType)
            : base(path, resourceType) { }

        public new UniTask<TResult>.Awaiter GetAwaiter () => completionSource.Task.GetAwaiter();

        public override void Cancel ()
        {
            completionSource.TrySetCanceled();
        }

        protected void SetResult (TResult result)
        {
            Result = result;
            completionSource.TrySetResult(Result);
        }

        protected override UniTask.Awaiter GetAwaiterImpl () => ((UniTask)completionSource.Task).GetAwaiter();
    }

    public abstract class LocateResourcesRunner<TResource> : ResourceRunner<IReadOnlyCollection<string>> 
        where TResource : UnityEngine.Object
    {
        protected LocateResourcesRunner (IResourceProvider provider, string path)
            : base(path, typeof(TResource)) { }
    }

    public abstract class LoadResourceRunner<TResource> : ResourceRunner<Resource<TResource>> 
        where TResource : UnityEngine.Object
    {
        protected LoadResourceRunner (IResourceProvider provider, string path)
            : base(path, typeof(TResource)) { }
    }

    public abstract class LocateFoldersRunner : ResourceRunner<IReadOnlyCollection<Folder>>
    {
        protected LocateFoldersRunner (IResourceProvider provider, string path)
            : base(path, typeof(Folder)) { }
    }
}
