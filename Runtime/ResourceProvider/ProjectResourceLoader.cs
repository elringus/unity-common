using System;
using UniRx.Async;
using UnityEngine;

namespace UnityCommon
{
    public class ProjectResourceLoader<TResource> : LoadResourceRunner<TResource> 
        where TResource : UnityEngine.Object
    {
        private readonly Action<string> logAction;
        private readonly ProjectResourceProvider.TypeRedirector redirector;

        public ProjectResourceLoader (IResourceProvider provider, string resourcePath, 
            ProjectResourceProvider.TypeRedirector redirector, Action<string> logAction) : base (provider, resourcePath)
        {
            this.redirector = redirector;
            this.logAction = logAction;
        }

        public override async UniTask RunAsync ()
        {
            var startTime = Time.time;

            var resourceType = redirector != null ? redirector.RedirectType : typeof(TResource);
            var asset = await Resources.LoadAsync(Path, resourceType);
            var assetName = System.IO.Path.GetFileNameWithoutExtension(Path);
            var obj = redirector is null ? asset as TResource : await redirector.ToSourceAsync<TResource>(asset, assetName);

            var result = new Resource<TResource>(Path, obj, Provider);
            SetResult(result);

            logAction?.Invoke($"Resource '{Path}' loaded over {Time.time - startTime:0.###} seconds.");
        }
    }
}
