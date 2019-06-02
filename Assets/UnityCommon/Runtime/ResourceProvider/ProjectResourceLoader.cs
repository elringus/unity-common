using System;
using System.Threading.Tasks;
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

        public override async Task RunAsync ()
        {
            var startTime = Time.time;

            var resourceType = redirector != null ? redirector.RedirectType : typeof(TResource);
            var resourceRequest = await Resources.LoadAsync(Path, resourceType);
            var obj = redirector is null ? resourceRequest.asset as TResource : await redirector.ToSourceAsync<TResource>(resourceRequest.asset);

            var result = new Resource<TResource>(Path, obj, Provider);
            SetResult(result);

            logAction?.Invoke($"Resource '{Path}' loaded over {Time.time - startTime:0.###} seconds.");
        }
    }
}
