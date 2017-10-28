using UnityEngine;
using UnityEngine.Events;

public class ResourceRequestRunner<T> : AsyncRunner where T : Object
{
    public event UnityAction<string, T> OnLoadComplete;

    public override bool CanBeInstantlyCompleted { get { return false; } }
    public ResourceRequest ResourceRequest { get; private set; }
    public string ResourcePath { get; private set; }

    public ResourceRequestRunner (MonoBehaviour coroutineContainer = null, UnityAction<string, T> onLoadComplete = null) :
        base(coroutineContainer, null)
    {
        OnLoadComplete += onLoadComplete;
    }

    public ResourceRequestRunner<T> Run (ResourceRequest resourceRequest, string path)
    {
        ResourceRequest = resourceRequest;
        ResourcePath = path;
        StartRunner(ResourceRequest);

        return this;
    }

    public override void Cancel ()
    {
        base.Cancel();

        ResourceRequest = null;
        OnLoadComplete.SafeInvoke(ResourcePath, null);
    }

    protected override void OnComplete ()
    {
        base.OnComplete();

        OnLoadComplete.SafeInvoke(ResourcePath, ResourceRequest.asset as T);
    }
}
