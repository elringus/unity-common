using UnityEngine;

public class ProjectResourceLoader<TResource> : AsyncRunner<Resource<TResource>> where TResource : class
{
    public override bool CanBeInstantlyCompleted { get { return false; } }
    public Resource<TResource> Resource { get { return Result; } private set { Result = value; } }

    private ResourceRequest resourceRequest;
    private ProjectResourceProvider.TypeRedirector redirector;

    public ProjectResourceLoader (Resource<TResource> resource, ProjectResourceProvider.TypeRedirector redirector = null,
        MonoBehaviour coroutineContainer = null) : base(coroutineContainer)
    {
        Resource = resource;
        this.redirector = redirector;
    }

    public override AsyncRunner<Resource<TResource>> Run ()
    {
        var resourceType = redirector != null ? redirector.RedirectType : typeof(TResource);
        YieldInstruction = resourceRequest = Resources.LoadAsync(Resource.Path, resourceType); 
        return base.Run();
    }

    protected override void HandleOnCompleted ()
    {
        Resource.Object = redirector != null ? redirector.ToSource<TResource>(resourceRequest.asset) : resourceRequest.asset as TResource;
        base.HandleOnCompleted();
    }
}
