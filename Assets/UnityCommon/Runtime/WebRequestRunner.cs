using System;
using UnityEngine;
using UnityEngine.Networking;

public class WebRequestRunner : AsyncRunner<UnityWebRequest>
{
    public UnityWebRequest WebRequest { get { return State; } private set { State = value; } }
    public override bool CanBeInstantlyCompleted { get { return false; } }

    public WebRequestRunner (UnityWebRequest webRequest, Action<UnityWebRequest> onResponse = null,
        MonoBehaviour coroutineContainer = null) : base(coroutineContainer, onResponse)
    {
        WebRequest = webRequest;
    }

    public override AsyncRunner<UnityWebRequest> Run ()
    {
        YieldInstruction = WebRequest.Send();
        return base.Run();
    }
}
