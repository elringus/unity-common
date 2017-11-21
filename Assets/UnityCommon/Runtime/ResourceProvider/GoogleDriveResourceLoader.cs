using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGoogleDrive;

public class GoogleDriveResourceLoader<TResource> : AsyncRunner where TResource : UnityEngine.Object
{
    public event Action<string, TResource> OnLoadComplete;

    public override bool CanBeInstantlyCompleted { get { return false; } }
    public string ResourcePath { get; private set; }
    public string RootPath { get; private set; }
    public bool IsLoading { get; private set; }

    private GoogleDriveFiles.DownloadRequest downloadRequest;
    private GoogleDriveFiles.ListRequest listRequest;
    private IRawConverter<TResource> converter;
    private byte[] rawData;

    public GoogleDriveResourceLoader (string rootPath, string resourcePath, IRawConverter<TResource> converter,
        MonoBehaviour coroutineContainer, Action<string, TResource> onLoadComplete = null) : base(coroutineContainer, null)
    {
        RootPath = rootPath;
        ResourcePath = resourcePath;
        this.converter = converter;
        if (onLoadComplete != null)
            OnLoadComplete += onLoadComplete;
    }

    public override void Run ()
    {
        if (IsLoading) return;
        IsLoading = true;

        CoroutineContainer.StartCoroutine(DownloadFileRoutine());
    }

    public override void Cancel ()
    {
        base.Cancel();

        if (downloadRequest != null)
        {
            downloadRequest.Abort();
            downloadRequest = null;
        }

        if (listRequest != null)
        {
            listRequest.Abort();
            listRequest = null;
        }

        OnLoadComplete.SafeInvoke(ResourcePath, null);
    }

    protected override void OnComplete ()
    {
        base.OnComplete();

        IsLoading = false;

        Debug.Assert(rawData != null);

        var resource = converter.Convert(rawData);
        OnLoadComplete.SafeInvoke(ResourcePath, resource);
    }

    private IEnumerator DownloadFileRoutine ()
    {
        var fullPath = string.Concat(RootPath, '/', ResourcePath);
        var fileName = fullPath.GetAfter("/");
        var parentNames = fullPath.GetBeforeLast("/").Split('/');

        // Resolving folder ids one by one to find id of the file's parent folder.
        var parendId = "root"; // 'root' is alias id for the root folder in Google Drive.
        for (int i = 0; i < parentNames.Length; i++)
        {
            listRequest = new GoogleDriveFiles.ListRequest();
            listRequest.Fields = new List<string> { "files(id)" };
            listRequest.Q = string.Format("'{0}' in parents and name = '{1}' and mimeType = 'application/vnd.google-apps.folder' and trashed = false", 
                parendId, parentNames[i]);

            yield return listRequest.Send();

            if (listRequest.IsError || listRequest.ResponseData.Files == null || listRequest.ResponseData.Files.Count == 0)
            {
                Debug.LogError(string.Format("Failed to retrieve {0} part of {1} resource from Google Drive.", parentNames[i], ResourcePath));
                yield break;
            }

            if (listRequest.ResponseData.Files.Count > 1)
                Debug.LogWarning(string.Format("Multiple '{0}' folders been found in Google Drive.", parentNames[i]));

            parendId = listRequest.ResponseData.Files[0].Id;
        }

        // Resolving file id.
        listRequest = new GoogleDriveFiles.ListRequest();
        listRequest.Fields = new List<string> { "files(id, modifiedTime)" };
        listRequest.Q = string.Format("'{0}' in parents and name = '{1}.{2}'", parendId, fileName, converter.Extension);

        yield return listRequest.Send();

        if (listRequest.IsError || listRequest.ResponseData.Files == null || listRequest.ResponseData.Files.Count == 0)
        {
            Debug.LogError(string.Format("Failed to retrieve {0} resource from Google Drive.", ResourcePath));
            yield break;
        }

        if (listRequest.ResponseData.Files.Count > 1)
            Debug.LogWarning(string.Format("Multiple '{0}' files been found in Google Drive.", ResourcePath));

        var fileId = listRequest.ResponseData.Files[0].Id;

        // TODO: Check if cached file has same modifiedTime.

        downloadRequest = new GoogleDriveFiles.DownloadRequest(fileId);

        yield return downloadRequest.Send();

        if (downloadRequest.IsError || downloadRequest.ResponseData.Content == null)
        {
            Debug.LogError(string.Format("Failed to download {0} resource from Google Drive.", ResourcePath));
            yield break;
        }

        rawData = downloadRequest.ResponseData.Content;

        // TODO: Cach the file.

        OnComplete();
    }
}
