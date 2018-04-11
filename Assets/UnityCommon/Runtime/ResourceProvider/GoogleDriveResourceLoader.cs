using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityGoogleDrive;
using UnityEngine.Networking;

public class GoogleDriveResourceLoader<TResource> : AsyncRunner<Resource<TResource>> where TResource : class
{
    [Serializable] struct CachedFileMeta { public string Id, ModifiedTime; }

    public override bool CanBeInstantlyCompleted { get { return false; } }
    public Resource<TResource> Resource { get { return Result; } private set { Result = value; } }
    public string RootPath { get; private set; }

    private const string CACHE_PATH = "GoogleDriveResources";
    private readonly List<Type> NATIVE_REQUEST_TYPES = new List<Type> { typeof(AudioClip), typeof(Texture2D) };

    private bool useNativeRequests;
    private GoogleDriveRequest downloadRequest;
    private GoogleDriveFiles.ListRequest listRequest;
    private IRawConverter<TResource> converter;
    private RawDataRepresentation usedRepresentation;
    private UnityGoogleDrive.Data.File fileMeta;
    private byte[] rawData;

    public GoogleDriveResourceLoader (string rootPath, Resource<TResource> resource, 
        IRawConverter<TResource> converter, MonoBehaviour coroutineContainer) : base(coroutineContainer)
    {
        RootPath = rootPath;
        Resource = resource;
        useNativeRequests = NATIVE_REQUEST_TYPES.Contains(typeof(TResource));
        this.converter = converter;
        usedRepresentation = new RawDataRepresentation();
    }

    public override AsyncRunner<Resource<TResource>> Run ()
    {
        // Corner case when loading folders.
        if (typeof(TResource) == typeof(Folder))
        {
            (Resource as Resource<Folder>).Object = new Folder(Resource.Path);
            base.HandleOnCompleted();
            return this;
        }

        return base.Run();
    }

    public override void Stop ()
    {
        base.Stop();

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

    protected override void HandleOnCompleted ()
    {
        if (!Resource.IsValid)
        {
            Debug.Assert(rawData != null);
            Resource.Object = converter.Convert(rawData);
        }

        if (downloadRequest != null) downloadRequest.Dispose();
        if (listRequest != null) listRequest.Dispose();

        base.HandleOnCompleted();
    }

    protected override IEnumerator AsyncRoutine ()
    {
        // 1. Load file metadata from Google Drive.
        var filePath = string.IsNullOrEmpty(RootPath) ? Resource.Path : string.Concat(RootPath, '/', Resource.Path);
        yield return GetFileMetaRoutine(filePath);
        if (fileMeta == null) { HandleOnCompleted(); yield break; }

        // 2. Check if cached version of the file could be used.
        yield return TryLoadFileCacheRoutine(fileMeta);

        // 3. Cached version is not valid or doesn't exist; download or export the file.
        if (rawData == null)
        {
            if (converter is IGoogleDriveConverter<TResource>) yield return ExportFile();
            else yield return DownloadFile();

            // 4. Cache the downloaded file.
            WriteFileCacheRoutine(fileMeta, rawData);
        }

        HandleOnCompleted();
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

            if (!IsResultFound(listRequest))
            {
                Debug.LogError(string.Format("Failed to retrieve {0} part of {1} resource from Google Drive.", parentNames[i], Resource.Path));
                yield break;
            }

            if (listRequest.ResponseData.Files.Count > 1)
                Debug.LogWarning(string.Format("Multiple '{0}' folders been found in Google Drive.", parentNames[i]));

            parendId = listRequest.ResponseData.Files[0].Id;
        }

        // 2. Searching the file and getting the metadata.
        var usedRepresentation = new RawDataRepresentation();
        foreach (var representation in converter.Representations)
        {
            listRequest = new GoogleDriveFiles.ListRequest();
            listRequest.Fields = new List<string> { "files(id, modifiedTime, mimeType)" };
            var fullName = string.IsNullOrEmpty(representation.Extension) ? fileName : string.Concat(fileName, ".", representation.Extension);
            listRequest.Q = string.Format("'{0}' in parents and name = '{1}' and mimeType = '{2}' and trashed = false", parendId, fullName, representation.MimeType);

            yield return listRequest.Send();

            if (IsResultFound(listRequest))
            {
                usedRepresentation = representation;
                break;
            }
        }

        if (!IsResultFound(listRequest))
        {
            Debug.LogError(string.Format("Failed to retrieve {0}.{1} resource from Google Drive.", Resource.Path, usedRepresentation.Extension));
            yield break;
        }

        if (listRequest.ResponseData.Files.Count > 1)
            Debug.LogWarning(string.Format("Multiple '{0}.{1}' files been found in Google Drive.", Resource.Path, usedRepresentation.Extension));

        fileMeta = listRequest.ResponseData.Files[0];

        // MP3 is not supported in native requests on the standalone platforms. Fallback to raw converters.
        #if UNITY_STANDALONE || UNITY_EDITOR
        if (EvaluateAudioTypeFromMime(fileMeta.MimeType) == AudioType.MPEG) useNativeRequests = false;
        #endif
    }

    private IEnumerator DownloadFile ()
    {
        Debug.Assert(fileMeta != null);

        if (useNativeRequests)
        {
            if (typeof(TResource) == typeof(AudioClip)) downloadRequest = GoogleDriveFiles.DownloadAudio(fileMeta);
            else if (typeof(TResource) == typeof(Texture2D)) downloadRequest = GoogleDriveFiles.DownloadTexture(fileMeta.Id, true);
        }
        else downloadRequest = new GoogleDriveFiles.DownloadRequest(fileMeta);

        yield return downloadRequest.SendNonGeneric();
        if (downloadRequest.IsError || downloadRequest.GetResourceData<UnityGoogleDrive.Data.File>().Content == null)
        {
            Debug.LogError(string.Format("Failed to download {0}.{1} resource from Google Drive.", Resource.Path, usedRepresentation.Extension));
            yield break;
        }

        if (useNativeRequests)
        {
            if (typeof(TResource) == typeof(AudioClip)) (Resource as Resource<AudioClip>).Object = downloadRequest.GetResourceData<UnityGoogleDrive.Data.AudioFile>().AudioClip;
            else if (typeof(TResource) == typeof(Texture2D)) (Resource as Resource<Texture2D>).Object = downloadRequest.GetResourceData<UnityGoogleDrive.Data.TextureFile>().Texture;
        }

        rawData = downloadRequest.GetResourceData<UnityGoogleDrive.Data.File>().Content;
    }

    private IEnumerator ExportFile ()
    {
        Debug.Assert(fileMeta != null && converter is IGoogleDriveConverter<TResource>);

        var gDriveConverter = converter as IGoogleDriveConverter<TResource>;

        downloadRequest = new GoogleDriveFiles.ExportRequest(fileMeta.Id, gDriveConverter.ExportMimeType);
        yield return downloadRequest.SendNonGeneric();
        if (downloadRequest.IsError || downloadRequest.GetResourceData<UnityGoogleDrive.Data.File>().Content == null)
        {
            Debug.LogError(string.Format("Failed to export {0} resource from Google Drive.", Resource.Path));
            yield break;
        }
        rawData = downloadRequest.GetResourceData<UnityGoogleDrive.Data.File>().Content;
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

        var filePath = string.Concat(Application.persistentDataPath, "/", CACHE_PATH, "/", fileMeta.Id);
        if (!string.IsNullOrEmpty(usedRepresentation.Extension))
            filePath += string.Concat(".", usedRepresentation.Extension);
        if (!File.Exists(filePath)) yield break;

        if (useNativeRequests)
        {
            // Web requests over IndexedDB are not supported; we should either use raw converters or disable caching.
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                // Binary convertion of the audio is fucked on WebGL (can't use buffers), so disable caching here.
                if (typeof(TResource) == typeof(AudioClip)) yield break;
                // Use raw converters for other native types.
                rawData = File.ReadAllBytes(filePath);
                yield break;
            }

            UnityWebRequest request = null;
            if (typeof(TResource) == typeof(AudioClip)) request = UnityWebRequestMultimedia.GetAudioClip(filePath, EvaluateAudioTypeFromMime(fileMeta.MimeType));
            else if (typeof(TResource) == typeof(Texture2D)) request = UnityWebRequestTexture.GetTexture(filePath, true);

            yield return request.SendWebRequest();

            if (typeof(TResource) == typeof(AudioClip)) (Resource as Resource<AudioClip>).Object = DownloadHandlerAudioClip.GetContent(request);
            else if (typeof(TResource) == typeof(Texture2D)) (Resource as Resource<Texture2D>).Object = DownloadHandlerTexture.GetContent(request);
            rawData = request.downloadHandler.data;
            request.Dispose();
        }
        else rawData = File.ReadAllBytes(filePath);
    }

    private void WriteFileCacheRoutine (UnityGoogleDrive.Data.File fileMeta, byte[] fileRawData)
    {
        var filePath = string.Concat(Application.persistentDataPath, "/", CACHE_PATH, "/", fileMeta.Id);
        if (!string.IsNullOrEmpty(usedRepresentation.Extension))
            filePath += string.Concat(".", usedRepresentation.Extension);

        Directory.CreateDirectory(Path.GetDirectoryName(filePath));

        File.WriteAllBytes(filePath, fileRawData);

        // Flush cached file writes to IndexedDB on WebGL.
        // https://forum.unity.com/threads/webgl-filesystem.294358/#post-1940712
        #if UNITY_WEBGL && !UNITY_EDITOR
        WebGLExtensions.SyncFs();
        #endif

        var cachedFileMeta = new CachedFileMeta() { Id = fileMeta.Id, ModifiedTime = fileMeta.ModifiedTime.Value.ToString("O") };
        var cachedFileMetaString = JsonUtility.ToJson(cachedFileMeta);
        PlayerPrefs.SetString(fileMeta.Id, cachedFileMetaString);
    }

    private bool IsResultFound (GoogleDriveFiles.ListRequest request)
    {
        return listRequest != null && !listRequest.IsError && listRequest.ResponseData.Files != null && listRequest.ResponseData.Files.Count > 0;
    }

    private AudioType EvaluateAudioTypeFromMime (string mimeType)
    {
        switch (mimeType)
        {
            case "audio/aiff": return AudioType.AIFF; 
            case "audio/mpeg": return AudioType.MPEG; 
            case "audio/ogg": return AudioType.OGGVORBIS; 
            case "video/ogg": return AudioType.OGGVORBIS; 
            case "audio/wav": return AudioType.WAV;
            default: return AudioType.UNKNOWN;
        }
    }
}
