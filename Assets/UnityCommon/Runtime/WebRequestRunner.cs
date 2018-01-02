using System;
using UnityEngine;
using UnityEngine.Networking;

public class WebRequestRunner : AsyncRunner<UnityWebRequest>
{
    public UnityWebRequest WebRequest { get { return Result; } private set { Result = value; } }
    public override bool CanBeInstantlyCompleted { get { return false; } }

    public WebRequestRunner (UnityWebRequest webRequest, Action<UnityWebRequest> onResponse = null,
        MonoBehaviour coroutineContainer = null) : base(coroutineContainer, onResponse)
    {
        WebRequest = webRequest;
    }

    public override AsyncRunner<UnityWebRequest> Run ()
    {
        #if UNITY_2017_3_OR_NEWER
        YieldInstruction = WebRequest.SendWebRequest();
        #else
        YieldInstruction = WebRequest.Send();
        #endif
        return base.Run();
    }
}
