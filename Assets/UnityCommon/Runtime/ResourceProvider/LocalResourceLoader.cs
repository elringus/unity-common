using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class LocalResourceLoader<TResource> : LoadResourceRunner<TResource> where TResource : class
{
    public string RootPath { get; private set; }

    private IRawConverter<TResource> converter;
    private RawDataRepresentation usedRepresentation;
    private byte[] rawData;

    public LocalResourceLoader (string rootPath, Resource<TResource> resource, IRawConverter<TResource> converter)
    {
        RootPath = rootPath;
        Resource = resource;
        this.converter = converter;
        usedRepresentation = new RawDataRepresentation();
    }

    public override async Task Run ()
    {
        await base.Run();

        // Corner case when loading folders.
        if (typeof(TResource) == typeof(Folder))
        {
            (Resource as Resource<Folder>).Object = new Folder(Resource.Path);
            HandleOnCompleted();
            return;
        }

        var filePath = string.IsNullOrEmpty(RootPath) ? Resource.Path : string.Concat(RootPath, '/', Resource.Path);
        filePath = string.Concat(Application.dataPath, "/", filePath);

        foreach (var representation in converter.Representations)
        {
            usedRepresentation = representation;
            var fullPath = string.Concat(filePath, ".", representation.Extension);
            if (!File.Exists(fullPath)) continue;

            using (var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            {
                rawData = new byte[fileStream.Length];
                await fileStream.ReadAsync(rawData, 0, (int)fileStream.Length);
            }

            break;
        }

        if (rawData == null) Debug.LogError(string.Format("Failed to load {0}.{1} resource using local file system: File not found.", Resource.Path, usedRepresentation.Extension));
        else Resource.Object = await converter.ConvertAsync(rawData);

        HandleOnCompleted();
    }
}
