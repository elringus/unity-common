using System;
using UnityEngine;
using UnityEngine.Networking;

public class WebRequestRunner : AsyncRunner
{
    public event Action<UnityWebRequest> OnResponse;

    public UnityWebRequest WebRequest { get; private set; }
    public override bool CanBeInstantlyCompleted { get { return false; } }

    public WebRequestRunner (UnityWebRequest webRequest, Action<UnityWebRequest> onResponse = null,
        MonoBehaviour coroutineContainer = null) : base(coroutineContainer, null)
    {
        WebRequest = webRequest;
        if (onResponse != null)
            OnResponse += onResponse;
    }

    public override void Run ()
    {
        YieldInstruction = WebRequest.Send();
        base.Run();
    }

    protected override void HandleOnCompleted ()
    {
        base.HandleOnCompleted();
        OnResponse.SafeInvoke(WebRequest);
    }
}
