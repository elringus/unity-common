using UnityEngine;

public class ProjectResourceLoader<TResource> : AsyncRunner where TResource : Object
{
    public override bool CanBeInstantlyCompleted { get { return false; } }
    public UnityResource<TResource> Resource { get; private set; }

    private ResourceRequest resourceRequest;

    public ProjectResourceLoader (UnityResource<TResource> resource, MonoBehaviour coroutineContainer = null) 
        : base(coroutineContainer, null)
    {
        Resource = resource;
    }

    public override void Run ()
    {
        resourceRequest = Resources.LoadAsync<TResource>(Resource.Path);
        StartRunner(resourceRequest);
    }

    public override void Cancel ()
    {
        base.Cancel();

        resourceRequest = null;
    }

    protected override void OnComplete ()
    {
        base.OnComplete();

        Resource.ProvideLoadedObject(resourceRequest.asset as TResource);
    }
}
