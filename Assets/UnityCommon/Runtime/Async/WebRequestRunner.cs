using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class WebRequestRunner : AsyncRunner
{
    public event UnityAction<UnityWebRequest> OnResponse;

    public override bool CanBeInstantlyCompleted { get { return false; } }

    private UnityWebRequest webRequest;

    public WebRequestRunner (MonoBehaviour coroutineContainer = null, UnityAction<UnityWebRequest> onResponse = null) :
        base(coroutineContainer, null)
    {
        OnResponse += onResponse;
    }

    public WebRequestRunner Run (UnityWebRequest webRequest)
    {
        this.webRequest = webRequest;
        StartRunner(webRequest.Send());

        return this;
    }

    protected override void OnComplete ()
    {
        base.OnComplete();
        OnResponse.SafeInvoke(webRequest);
    }
}
