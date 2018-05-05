using System.Threading.Tasks;
using UnityEngine;

public class ProjectResourceLoader<TResource> : LoadResourceRunner<TResource> where TResource : class
{
    private ResourceRequest resourceRequest;
    private ProjectResourceProvider.TypeRedirector redirector;

    public ProjectResourceLoader (Resource<TResource> resource, ProjectResourceProvider.TypeRedirector redirector = null)
    {
        Resource = resource;
        this.redirector = redirector;
    }

    public override async Task Run ()
    {
        await base.Run(); 

        // Corner case when loading folders.
        if (typeof(TResource) == typeof(Folder))
        {
            (Resource as Resource<Folder>).Object = new Folder(Resource.Path);
            base.HandleOnCompleted();
            return;
        }

        var resourceType = redirector != null ? redirector.RedirectType : typeof(TResource);
        resourceRequest = await Resources.LoadAsync(Resource.Path, resourceType);
        HandleOnCompleted();
    }

    protected override void HandleOnCompleted ()
    {
        Resource.Object = redirector != null ? redirector.ToSource<TResource>(resourceRequest.asset) : resourceRequest.asset as TResource;
        base.HandleOnCompleted();
    }
}
