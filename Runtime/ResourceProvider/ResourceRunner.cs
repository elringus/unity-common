using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace UnityCommon
{
    public abstract class ResourceRunner
    {
        public readonly IResourceProvider Provider;
        public readonly string Path;
        public readonly Type ResourceType;

        public ResourceRunner (IResourceProvider provider, string path, Type resourceType)
        {
            Provider = provider;
            Path = path;
            ResourceType = resourceType;
        }

        public TaskAwaiter GetAwaiter () => GetAwaiterImpl();

        public abstract Task RunAsync ();
        public abstract void Cancel ();

        protected abstract TaskAwaiter GetAwaiterImpl ();
    }

    public abstract class ResourceRunner<TResult> : ResourceRunner
    {
        public TResult Result { get; private set; }

        private TaskCompletionSource<TResult> completionSource = new TaskCompletionSource<TResult>();

        public ResourceRunner (IResourceProvider provider, string path, Type resourceType)
            : base(provider, path, resourceType) { }

        public new TaskAwaiter<TResult> GetAwaiter () => completionSource.Task.GetAwaiter();

        public override void Cancel ()
        {
            completionSource.TrySetCanceled();
        }

        protected void SetResult (TResult result)
        {
            Result = result;
            completionSource.TrySetResult(Result);
        }

        protected override TaskAwaiter GetAwaiterImpl () => ((Task)completionSource.Task).GetAwaiter();
    }

    public abstract class LocateResourcesRunner<TResource> : ResourceRunner<IEnumerable<string>> 
        where TResource : UnityEngine.Object
    {
        public LocateResourcesRunner (IResourceProvider provider, string path)
            : base(provider, path, typeof(TResource)) { }
    }

    public abstract class LoadResourceRunner<TResource> : ResourceRunner<Resource<TResource>> 
        where TResource : UnityEngine.Object
    {
        public LoadResourceRunner (IResourceProvider provider, string path)
            : base(provider, path, typeof(TResource)) { }
    }

    public abstract class LocateFoldersRunner : ResourceRunner<IEnumerable<Folder>>
    {
        public LocateFoldersRunner (IResourceProvider provider, string path)
            : base(provider, path, typeof(Folder)) { }
    }
}
