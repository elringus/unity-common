using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGoogleDrive;

public class GoogleDriveResourceLocator<TResource> : AsyncRunner<List<Resource<TResource>>> where TResource : class
{
    public override bool CanBeInstantlyCompleted { get { return false; } }
    public List<Resource<TResource>> LocatedResources { get { return Result; } private set { Result = value; } }
    public string RootPath { get; private set; }
    public string ResourcesPath { get; private set; }

    private GoogleDriveFiles.ListRequest listRequest;
    private IRawConverter<TResource> converter;

    public GoogleDriveResourceLocator (string rootPath, string resourcesPath, IRawConverter<TResource> converter, 
        MonoBehaviour coroutineContainer) : base(coroutineContainer)
    {
        RootPath = rootPath;
        ResourcesPath = resourcesPath;
        this.converter = converter;
    }

    public override void Stop ()
    {
        base.Stop();

        if (listRequest != null)
        {
            listRequest.Abort();
            listRequest = null;
        }
    }

    protected override IEnumerator AsyncRoutine ()
    {
        var folderPath = string.IsNullOrEmpty(RootPath) ? ResourcesPath : string.Concat(RootPath, '/', ResourcesPath);
        var parentNames = folderPath.Split('/');

        // 1. Resolving folder ids one by one to find id of the last one.
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
                Debug.LogError(string.Format("Failed to retrieve {0} part of {1} resource from Google Drive.", parentNames[i], folderPath));
                yield break;
            }

            if (listRequest.ResponseData.Files.Count > 1)
                Debug.LogWarning(string.Format("Multiple '{0}' folders been found in Google Drive.", parentNames[i]));

            parendId = listRequest.ResponseData.Files[0].Id;
        }

        // 2. Searching for the files in the folder.
        var results = new Dictionary<RawDataRepresentation, List<UnityGoogleDrive.Data.File>>(); 
        foreach (var representation in converter.Representations)
        {
            listRequest = new GoogleDriveFiles.ListRequest();
            listRequest.Fields = new List<string> { "files(name)" };
            listRequest.Q = string.Format("'{0}' in parents and mimeType = '{1}' and trashed = false", parendId, representation.MimeType);

            yield return listRequest.Send();

            if (IsResultFound(listRequest))
                results.Add(representation, listRequest.ResponseData.Files);
        }

        if (results.Count == 0)
        {
            Debug.LogError(string.Format("Failed to locate files at '{0}' in Google Drive.", folderPath));
            yield break;
        }

        // 3. Create resources using located files.
        LocatedResources = new List<Resource<TResource>>();
        foreach (var result in results)
        {
            foreach (var file in result.Value)
            {
                var fileName = string.IsNullOrEmpty(result.Key.Extension) ? file.Name : file.Name.GetBeforeLast(".");
                var filePath = string.Concat(ResourcesPath, '/', fileName);
                var fileResource = new Resource<TResource>(filePath);
                LocatedResources.Add(fileResource);
            }
        }

        HandleOnCompleted();
    }

    private bool IsResultFound (GoogleDriveFiles.ListRequest request)
    {
        return listRequest != null && !listRequest.IsError && listRequest.ResponseData.Files != null && listRequest.ResponseData.Files.Count > 0;
    }
}
