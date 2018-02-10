using System.Collections;
using System.IO;
using UnityEngine;

public class LocalResourceLoader<TResource> : AsyncRunner<Resource<TResource>> where TResource : class
{
    public override bool CanBeInstantlyCompleted { get { return false; } }
    public Resource<TResource> Resource { get { return Result; } private set { Result = value; } }
    public string RootPath { get; private set; }

    private IRawConverter<TResource> converter;
    private RawDataRepresentation usedRepresentation;
    private byte[] rawData;

    public LocalResourceLoader (string rootPath, Resource<TResource> resource,
        IRawConverter<TResource> converter, MonoBehaviour coroutineContainer) : base(coroutineContainer)
    {
        RootPath = rootPath;
        Resource = resource;
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

    protected override void HandleOnCompleted ()
    {
        Debug.Assert(rawData != null);
        Resource.Object = converter.Convert(rawData);
        base.HandleOnCompleted();
    }

    protected override IEnumerator AsyncRoutine ()
    {
        var filePath = string.IsNullOrEmpty(RootPath) ? Resource.Path : string.Concat(RootPath, '/', Resource.Path);
        filePath = string.Concat(Application.dataPath, "/", filePath);

        foreach (var representation in converter.Representations)
        {
            var fullPath = string.Concat(filePath, ".", representation.Extension);
            if (!File.Exists(fullPath)) continue;
            rawData = File.ReadAllBytes(fullPath);
            usedRepresentation = representation;
            break;
        }

        if (rawData == null)
            Debug.LogError(string.Format("Failed to load {0}.{1} resource using local file system: File not found.", Resource.Path, usedRepresentation.Extension));

        HandleOnCompleted();

        yield break;
    }
}
