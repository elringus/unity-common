using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityCommon
{
    public class ProjectFolderLocator : LocateFoldersRunner
    {
        private readonly IReadOnlyDictionary<string, Type> projectResources;

        public ProjectFolderLocator (IResourceProvider provider, string resourcesPath, IReadOnlyDictionary<string, Type> projectResources)
            : base(provider, resourcesPath ?? string.Empty)
        {
            this.projectResources = projectResources;
        }

        public override UniTask RunAsync ()
        {
            var locatedFolders = LocateProjectFolders(Path, projectResources);
            SetResult(locatedFolders);
            return UniTask.CompletedTask;
        }

        public static IReadOnlyCollection<Folder> LocateProjectFolders (string resourcesPath, IReadOnlyDictionary<string, Type> projectResources)
        {
            return projectResources.Keys.LocateFolderPathsAtFolder(resourcesPath)
                .Select(p => new Folder(p)).ToList();
        }
    }
}
