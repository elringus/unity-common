using UnityEngine;

public class ProjectResourceLoader<TResource> : AsyncRunner<UnityResource<TResource>> where TResource : Object
{
    public override bool CanBeInstantlyCompleted { get { return false; } }
    public UnityResource<TResource> Resource { get { return State; } private set { State = value; } }

    private ResourceRequest resourceRequest;

    public ProjectResourceLoader (UnityResource<TResource> resource, MonoBehaviour coroutineContainer = null) 
        : base(coroutineContainer)
    {
        Resource = resource;
    }

    public override AsyncRunner<UnityResource<TResource>> Run ()
    {
        YieldInstruction = Resources.LoadAsync<TResource>(Resource.Path);
        return base.Run();
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
