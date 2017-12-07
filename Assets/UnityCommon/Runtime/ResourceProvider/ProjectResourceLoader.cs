using UnityEngine;

public class ProjectResourceLoader<TResource> : AsyncRunner where TResource : Object
{
    public override bool CanBeInstantlyCompleted { get { return false; } }
    public UnityResource<TResource> Resource { get; private set; }

    private ResourceRequest resourceRequest;

    public ProjectResourceLoader (UnityResource<TResource> resource, MonoBehaviour coroutineContainer = null) 
        : base(coroutineContainer)
    {
        Resource = resource;
    }

    public override void Run ()
    {
        YieldInstruction = Resources.LoadAsync<TResource>(Resource.Path);
        base.Run();
    }

    public override void Stop ()
    {
        base.Stop();

        resourceRequest = null;
    }

    protected override void HandleOnCompleted ()
    {
        Resource.Object = resourceRequest.asset as TResource;
        base.HandleOnCompleted();
    }
}
