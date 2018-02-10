using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class LocalResourceLocator<TResource> : AsyncRunner<List<Resource<TResource>>> where TResource : class
{
    public override bool CanBeInstantlyCompleted { get { return false; } }
    public List<Resource<TResource>> LocatedResources { get { return Result; } private set { Result = value; } }
    public string RootPath { get; private set; }
    public string ResourcesPath { get; private set; }

    private IRawConverter<TResource> converter;

    public LocalResourceLocator (string rootPath, string resourcesPath, IRawConverter<TResource> converter,
        MonoBehaviour coroutineContainer) : base(coroutineContainer)
    {
        RootPath = rootPath;
        ResourcesPath = resourcesPath;
        this.converter = converter;
    }

    protected override IEnumerator AsyncRoutine ()
    {
        LocatedResources = new List<Resource<TResource>>();

        // 1. Resolving parent folder.
        var folderPath = Application.dataPath;
        if (!string.IsNullOrEmpty(RootPath) && !string.IsNullOrEmpty(ResourcesPath))
            folderPath += string.Concat('/', RootPath, '/', ResourcesPath);
        else if (string.IsNullOrEmpty(RootPath)) folderPath += string.Concat('/', ResourcesPath); 
        else folderPath += string.Concat('/', RootPath);
        var parendFolder = new DirectoryInfo(folderPath);
        if (!parendFolder.Exists)
        {
            HandleOnCompleted();
            yield break;
        }

        // Corner case for folders.
        if (typeof(TResource) == typeof(Folder))
        {
            foreach (var dir in parendFolder.GetDirectories())
            {
                var path = dir.FullName.Replace("\\", "/").GetAfterFirst(RootPath + "/");
                var resource = new Resource<Folder>(path, new Folder(path));
                LocatedResources.Add(resource as Resource<TResource>);
            }
            HandleOnCompleted();
            yield break;
        }

        // 2. Searching for the files in the folder.
        var results = new Dictionary<RawDataRepresentation, List<FileInfo>>();
        foreach (var representation in converter.Representations)
        {
            var files = parendFolder.GetFiles(string.Concat("*.", representation.Extension)).ToList();
            if (files != null && files.Count > 0) results.Add(representation, files);
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

        HandleOnCompleted();

        yield break;
    }
}
