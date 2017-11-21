using System;
using UnityEngine;

public class ResourceRequestRunner<T> : AsyncRunner where T : UnityEngine.Object
{
    public event Action<string, T> OnLoadComplete;

    public override bool CanBeInstantlyCompleted { get { return false; } }
    public ResourceRequest ResourceRequest { get; private set; }
    public string ResourcePath { get; private set; }

    public ResourceRequestRunner (ResourceRequest resourceRequest, string path, MonoBehaviour coroutineContainer = null, 
        Action<string, T> onLoadComplete = null) : base(coroutineContainer, null)
    {
        ResourceRequest = resourceRequest;
        ResourcePath = path;
        if (onLoadComplete != null)
            OnLoadComplete += onLoadComplete;
    }

    public override void Run ()
    {
        StartRunner(ResourceRequest);
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
