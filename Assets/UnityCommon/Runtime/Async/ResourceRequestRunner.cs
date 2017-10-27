using UnityEngine;
using UnityEngine.Events;

public class ResourceRequestRunner : AsyncRunner
{
    public event UnityAction<ResourceRequestRunner> OnResponse;

    public override bool CanBeInstantlyCompleted { get { return false; } }

    public ResourceRequest ResourceRequest { get; private set; }
    public string ResourcePath { get; private set; }

    public ResourceRequestRunner (MonoBehaviour coroutineContainer = null, UnityAction<ResourceRequestRunner> onResponse = null) :
        base(coroutineContainer, null)
    {
        OnResponse += onResponse;
    }

    public ResourceRequestRunner Run (ResourceRequest resourceRequest, string path)
    {
        ResourceRequest = resourceRequest;
        ResourcePath = path;
        StartRunner(ResourceRequest);

        return this;
    }

    protected override void OnComplete ()
    {
        base.OnComplete();
        OnResponse.SafeInvoke(this);
    }
}
