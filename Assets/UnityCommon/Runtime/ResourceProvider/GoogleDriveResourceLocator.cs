using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGoogleDrive;

public class GoogleDriveResourceLocator<TResource> : AsyncRunner<List<UnityResource<TResource>>> where TResource : Object
{
    public override bool CanBeInstantlyCompleted { get { return false; } }
    public List<UnityResource<TResource>> LocatedResources { get { return State; } private set { State = value; } }
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
        var folderPath = string.Concat(RootPath, '/', ResourcesPath);
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

            if (listRequest.IsError || listRequest.ResponseData.Files == null || listRequest.ResponseData.Files.Count == 0)
            {
                Debug.LogError(string.Format("Failed to retrieve {0} part of {1} resource from Google Drive.", parentNames[i], folderPath));
                yield break;
            }

            if (listRequest.ResponseData.Files.Count > 1)
                Debug.LogWarning(string.Format("Multiple '{0}' folders been found in Google Drive.", parentNames[i]));

            parendId = listRequest.ResponseData.Files[0].Id;
        }

        // 2. Searching for the files in the folder.
        listRequest = new GoogleDriveFiles.ListRequest();
        listRequest.Fields = new List<string> { "files(name)" };
        listRequest.Q = string.Format("'{0}' in parents and mimeType = '{1}'", parendId, converter.MimeType);

        yield return listRequest.Send();

        if (listRequest.IsError || listRequest.ResponseData.Files == null || listRequest.ResponseData.Files.Count == 0)
        {
            Debug.LogError(string.Format("Failed to locate files at '{0}' in Google Drive.", folderPath));
            yield break;
        }

        // 3. Create resources using located files.
        LocatedResources = new List<UnityResource<TResource>>();
        foreach (var file in listRequest.ResponseData.Files)
        {
            var filePath = string.Concat(ResourcesPath, '/', file.Name.GetBeforeLast("."));
            var fileResource = new UnityResource<TResource>(filePath);
            LocatedResources.Add(fileResource);
        }

        HandleOnCompleted();
    }
}
