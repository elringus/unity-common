using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityGoogleDrive;

public class GoogleDriveResourceLocator<TResource> : LocateResourcesRunner<TResource> where TResource : class
{
    public string RootPath { get; private set; }
    public string ResourcesPath { get; private set; }

    private GoogleDriveFiles.ListRequest listRequest;
    private IRawConverter<TResource> converter;

    public GoogleDriveResourceLocator (string rootPath, string resourcesPath, IRawConverter<TResource> converter)
    {
        RootPath = rootPath;
        ResourcesPath = resourcesPath;
        this.converter = converter;
    }

    public override async Task Run ()
    {
        await base.Run();

        LocatedResources = new List<Resource<TResource>>();

        // 1. Resolving folder ids one by one to find id of the last one.
        var parendId = "root"; // 'root' is alias id for the root folder in Google Drive.
        if (!string.IsNullOrEmpty(RootPath) || !string.IsNullOrEmpty(ResourcesPath))
        {
            var folderPath = string.Empty;
            if (!string.IsNullOrEmpty(RootPath) && !string.IsNullOrEmpty(ResourcesPath))
                folderPath = string.Concat(RootPath, '/', ResourcesPath);
            else if (string.IsNullOrEmpty(RootPath)) folderPath = ResourcesPath;
            else folderPath = RootPath;

            var parentNames = folderPath.Split('/');

            for (int i = 0; i < parentNames.Length; i++)
            {
                listRequest = new GoogleDriveFiles.ListRequest();
                listRequest.Fields = new List<string> { "files(id)" };
                listRequest.Q = string.Format("'{0}' in parents and name = '{1}' and mimeType = 'application/vnd.google-apps.folder' and trashed = false",
                    parendId, parentNames[i]);

                await listRequest.Send();

                if (!IsResultFound(listRequest))
                {
                    //Debug.LogError(string.Format("Failed to retrieve {0} part of {1} resource from Google Drive.", parentNames[i], folderPath));
                    HandleOnCompleted();
                    return;
                }

                if (listRequest.ResponseData.Files.Count > 1)
                    Debug.LogWarning(string.Format("Multiple '{0}' folders been found in Google Drive.", parentNames[i]));

                parendId = listRequest.ResponseData.Files[0].Id;
            }
        }

        // 2. Searching for the files in the folder.
        var results = new Dictionary<RawDataRepresentation, List<UnityGoogleDrive.Data.File>>(); 
        foreach (var representation in converter.Representations)
        {
            listRequest = new GoogleDriveFiles.ListRequest();
            listRequest.Fields = new List<string> { "files(name)" };
            listRequest.Q = string.Format("'{0}' in parents and mimeType = '{1}' and trashed = false", parendId, representation.MimeType);

            await listRequest.Send();

            if (IsResultFound(listRequest))
                results.Add(representation, listRequest.ResponseData.Files);
        }

        // 3. Create resources using located files.
        foreach (var result in results)
        {
            foreach (var file in result.Value)
            {
                var fileName = string.IsNullOrEmpty(result.Key.Extension) ? file.Name : file.Name.GetBeforeLast(".");
                var filePath = string.IsNullOrEmpty(ResourcesPath) ? fileName : string.Concat(ResourcesPath, '/', fileName);
                var fileResource = new Resource<TResource>(filePath);
                LocatedResources.Add(fileResource);
            }
        }

        if (listRequest != null) listRequest.Dispose();

        HandleOnCompleted();
    }

    public override void Cancel ()
    {
        base.Cancel();

        if (listRequest != null)
        {
            listRequest.Abort();
            listRequest = null;
        }
    }

    private bool IsResultFound (GoogleDriveFiles.ListRequest request)
    {
        return listRequest != null && !listRequest.IsError && listRequest.ResponseData.Files != null && listRequest.ResponseData.Files.Count > 0;
    }
}
