using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProjectResourceLocator<TResource> : AsyncRunner<List<Resource<TResource>>> where TResource : class
{
    public override bool CanBeInstantlyCompleted { get { return false; } }
    public List<Resource<TResource>> LocatedResources { get { return Result; } private set { Result = value; } }
    public string ResourcesPath { get; private set; }

    private ProjectResourceProvider.TypeRedirector redirector;

    public ProjectResourceLocator (string resourcesPath, ProjectResourceProvider.TypeRedirector redirector = null, 
        MonoBehaviour coroutineContainer = null) : base(coroutineContainer)
    {
        ResourcesPath = resourcesPath ?? string.Empty;
        this.redirector = redirector;
    }

    protected override IEnumerator AsyncRoutine ()
    {
        // Corner case when locating folders (unity doesn't see folder as a resource).
        if (typeof(TResource) == typeof(Folder))
        {
            LocatedResources = LocateAllResourceFolders().FindAllAtPath(ResourcesPath)
                .Select(f => new Resource<Folder>(f.Path, f) as Resource<TResource>).ToList();
            HandleOnCompleted();
            yield break;
        }

        // TODO: Make this async (if possible, LoadAllAsync doesn't exist).
        var redirectType = redirector != null ? redirector.RedirectType : typeof(TResource);
        var objects = Resources.LoadAll(ResourcesPath, redirectType);
        LocatedResources = objects.Select(r => new Resource<TResource>(string.Concat(ResourcesPath, "/", r.name),
            redirector != null ? redirector.ToSource<TResource>(r) : r as TResource)).ToList();

        HandleOnCompleted();

        yield break;
    }

    #if UNITY_EDITOR
    private static List<Folder> LocateAllResourceFolders ()
    {
        var folders = new List<Folder>();
        WalkDirectoryTree(new System.IO.DirectoryInfo(Application.dataPath), ref folders, false);
        return folders;
    }

    private static void WalkDirectoryTree (System.IO.DirectoryInfo directory, ref List<Folder> outFolders, bool isInsideResources)
    {
        var subDirs = directory.GetDirectories();
        foreach (var dirInfo in subDirs)
        {
            if (!isInsideResources && dirInfo.Name != "Resources") continue;
            if (!isInsideResources && dirInfo.Name == "Resources") WalkDirectoryTree(dirInfo, ref outFolders, true);

            if (isInsideResources)
            {
                var folder = new Folder(dirInfo.FullName.Replace("\\", "/").GetAfterFirst("/Resources"));
                outFolders.Add(folder);
                WalkDirectoryTree(dirInfo, ref outFolders, true);
            }
        }
    }
    #endif
}
