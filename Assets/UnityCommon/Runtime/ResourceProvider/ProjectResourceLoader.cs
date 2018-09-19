using System;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityCommon
{
    public class ProjectResourceLoader<TResource> : LoadResourceRunner<TResource>
    {
        private Action<string> logAction;
        private ResourceRequest resourceRequest;
        private ProjectResourceProvider.TypeRedirector redirector;

        public ProjectResourceLoader (Resource<TResource> resource,
            ProjectResourceProvider.TypeRedirector redirector, Action<string> logAction)
        {
            Resource = resource;
            this.redirector = redirector;
            this.logAction = logAction;
        }

        public override async Task Run ()
        {
            await base.Run();

            var startTime = Time.time;

            // Corner case when loading folders.
            if (typeof(TResource) == typeof(Folder))
            {
                (Resource as Resource<Folder>).Object = new Folder(Resource.Path);
                base.HandleOnCompleted();
                return;
            }

            var resourceType = redirector != null ? redirector.RedirectType : typeof(TResource);
            resourceRequest = await Resources.LoadAsync(Resource.Path, resourceType);
            Resource.Object = redirector != null ? await redirector.ToSourceAsync<TResource>(resourceRequest.asset) : (TResource)(object)resourceRequest.asset;

            logAction?.Invoke($"Resource '{Resource.Path}' loaded over {Time.time - startTime:0.###} seconds.");

            HandleOnCompleted();
        }
    }
}
