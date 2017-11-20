using System;
using UnityEngine;
using UnityGoogleDrive;

public class GoogleDriveResourceLoader<T> : AsyncRunner where T : UnityEngine.Object
{
    public event Action<string, T> OnLoadComplete;

    public override bool CanBeInstantlyCompleted { get { return false; } }
    public GoogleDriveFiles.DownloadRequest DownloadRequest { get; private set; }
    public string ResourcePath { get; private set; }
    public bool IsLoading { get; private set; }
    public byte[] RawData { get; private set; }

    public GoogleDriveResourceLoader (string resourcePath, MonoBehaviour coroutineContainer = null, 
        Action<string, T> onLoadComplete = null) : base(coroutineContainer, null)
    {
        ResourcePath = resourcePath;
        if (onLoadComplete != null)
            OnLoadComplete += onLoadComplete;
    }

    public void Load ()
    {
        if (IsLoading) return;
        IsLoading = true;

        // create download request
    }

    public override void Cancel ()
    {
        base.Cancel();

        DownloadRequest.Abort();
        DownloadRequest = null;
        OnLoadComplete.SafeInvoke(ResourcePath, null);
    }

    protected override void OnComplete ()
    {
        base.OnComplete();

        IsLoading = false;

        Debug.Assert(RawData != null);

        // convert raw data to unity object.

        //OnLoadComplete.SafeInvoke(ResourcePath, ... as T);
    }

    private void HandleDownloadDone (UnityGoogleDrive.Data.File file)
    {

    }
}
