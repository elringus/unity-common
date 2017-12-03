using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityGoogleDrive;

public class GoogleDriveResourceLoader<TResource> : AsyncRunner where TResource : UnityEngine.Object
{
    [Serializable] struct CachedFileMeta { public string Id, ModifiedTime; }

    public override bool CanBeInstantlyCompleted { get { return false; } }
    public UnityResource<TResource> Resource { get; private set; }
    public string RootPath { get; private set; }
    public bool IsLoading { get; private set; }

    private const string CACHE_PATH = "GoogleDriveResources";

    private GoogleDriveFiles.DownloadRequest downloadRequest;
    private GoogleDriveFiles.ListRequest listRequest;
    private IRawConverter<TResource> converter;
    private UnityGoogleDrive.Data.File fileMeta;
    private byte[] rawData;

    public GoogleDriveResourceLoader (string rootPath, UnityResource<TResource> resource, 
        IRawConverter<TResource> converter, MonoBehaviour coroutineContainer) : base(coroutineContainer, null)
    {
        RootPath = rootPath;
        Resource = resource;
        this.converter = converter;
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
    }

    protected override void OnComplete ()
    {
        base.OnComplete();

        IsLoading = false;

        Debug.Assert(rawData != null);

        var resource = converter.Convert(rawData);
        Resource.ProvideLoadedObject(resource);
    }

    private IEnumerator DownloadFileRoutine ()
    {
        // 1. Load file metadata from Google Drive.
        var filePath = string.Concat(RootPath, '/', Resource.Path);
        yield return GetFileMetaRoutine(filePath);
        if (fileMeta == null) yield break;

        // 2. Check if cached version of the file could be used.
        yield return TryLoadFileCacheRoutine(fileMeta);

        // 3. Cached version is not valid or doesn't exist; download the file.
        if (rawData == null)
        {
            downloadRequest = new GoogleDriveFiles.DownloadRequest(fileMeta.Id);
            yield return downloadRequest.Send();
            if (downloadRequest.IsError || downloadRequest.ResponseData.Content == null)
            {
                Debug.LogError(string.Format("Failed to download {0} resource from Google Drive.", Resource.Path));
                yield break;
            }
            rawData = downloadRequest.ResponseData.Content;

            // 4. Cache the downloaded file.
            yield return WriteFileCacheRoutine(fileMeta, rawData);
        }

        OnComplete();
    }

    private IEnumerator GetFileMetaRoutine (string filePath)
    {
        fileMeta = null;

        var fileName = filePath.GetAfter("/");
        var parentNames = filePath.GetBeforeLast("/").Split('/');

        // 1. Resolving folder ids one by one to find id of the file's parent folder.
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
                Debug.LogError(string.Format("Failed to retrieve {0} part of {1} resource from Google Drive.", parentNames[i], Resource.Path));
                yield break;
            }

            if (listRequest.ResponseData.Files.Count > 1)
                Debug.LogWarning(string.Format("Multiple '{0}' folders been found in Google Drive.", parentNames[i]));

            parendId = listRequest.ResponseData.Files[0].Id;
        }

        // 2. Searching the file and getting the metadata.
        listRequest = new GoogleDriveFiles.ListRequest();
        listRequest.Fields = new List<string> { "files(id, modifiedTime)" };
        listRequest.Q = string.Format("'{0}' in parents and name = '{1}.{2}'", parendId, fileName, converter.Extension);

        yield return listRequest.Send();

        if (listRequest.IsError || listRequest.ResponseData.Files == null || listRequest.ResponseData.Files.Count == 0)
        {
            Debug.LogError(string.Format("Failed to retrieve {0} resource from Google Drive.", Resource.Path));
            yield break;
        }

        if (listRequest.ResponseData.Files.Count > 1)
            Debug.LogWarning(string.Format("Multiple '{0}' files been found in Google Drive.", Resource.Path));

        fileMeta = listRequest.ResponseData.Files[0];
    }

    private IEnumerator TryLoadFileCacheRoutine (UnityGoogleDrive.Data.File fileMeta)
    {
        rawData = null;

        if (!PlayerPrefs.HasKey(fileMeta.Id)) yield break;

        var cachedFileMetaString = PlayerPrefs.GetString(fileMeta.Id);
        var cachedFileMeta = JsonUtility.FromJson<CachedFileMeta>(cachedFileMetaString);
        var modifiedTime = DateTime.ParseExact(cachedFileMeta.ModifiedTime, "O",
            CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

        if (fileMeta.ModifiedTime > modifiedTime) yield break;

        var filePath = string.Concat(Application.persistentDataPath, "/", CACHE_PATH, "/", fileMeta.Id, ".", converter.Extension);
        if (!File.Exists(filePath)) yield break;

        var fileStream = File.OpenRead(filePath);
        rawData = new byte[fileStream.Length];
        var asyncRead = fileStream.BeginRead(rawData, 0, (int)fileStream.Length, asyncResult => {
            fileStream.EndRead(asyncResult);
        }, null);

        yield return asyncRead;

        fileStream.Dispose();
    }

    private IEnumerator WriteFileCacheRoutine (UnityGoogleDrive.Data.File fileMeta, byte[] fileRawData)
    {
        var filePath = string.Concat(Application.persistentDataPath, "/", CACHE_PATH, "/", fileMeta.Id, ".", converter.Extension);
        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        var fileStream = File.Create(filePath, fileRawData.Length);
        var asyncWrite = fileStream.BeginWrite(fileRawData, 0, fileRawData.Length, asyncResult => {
            fileStream.EndWrite(asyncResult);
        }, null);

        yield return asyncWrite;

        fileStream.Dispose();

        // Flush cached file writes to IndexedDB on WebGL.
        // https://forum.unity.com/threads/webgl-filesystem.294358/#post-1940712
        #if UNITY_WEBGL && !UNITY_EDITOR
        WebGLExtensions.SyncFs();
        #endif

        var cachedFileMeta = new CachedFileMeta() { Id = fileMeta.Id, ModifiedTime = fileMeta.ModifiedTime.Value.ToString("O") };
        var cachedFileMetaString = JsonUtility.ToJson(cachedFileMeta);
        PlayerPrefs.SetString(fileMeta.Id, cachedFileMetaString);
    }
}
