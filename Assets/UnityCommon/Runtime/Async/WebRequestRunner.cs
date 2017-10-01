using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class OnWebResponse : UnityEvent<UnityWebRequest> { }

public class WebRequestRunner : AsyncRunner
{
    public readonly OnWebResponse OnResponse = new OnWebResponse();

    public override bool CanBeInstantlyCompleted { get { return false; } }

    private UnityWebRequest webRequest;

    public WebRequestRunner (MonoBehaviour coroutineContainer = null, UnityAction<UnityWebRequest> onResponse = null) :
        base(coroutineContainer, null)
    {
        if (onResponse != null)
            OnResponse.AddListener(onResponse);
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
        OnResponse.Invoke(webRequest);
    }
}
