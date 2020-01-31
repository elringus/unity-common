using System.Collections.Generic;
using System.Linq;
using UniRx.Async;

namespace UnityCommon
{
    public class ProjectFolderLocator : LocateFoldersRunner
    {
        private readonly ProjectResources projectResources;

        public ProjectFolderLocator (IResourceProvider provider, string resourcesPath, ProjectResources projectResources)
            : base (provider, resourcesPath ?? string.Empty)
        {
            this.projectResources = projectResources;
        }

        public override UniTask RunAsync ()
        {
            var locatedFolders = LocateProjectFolders(Path, projectResources);
            SetResult(locatedFolders);
            return UniTask.CompletedTask;
        }

        public static List<Folder> LocateProjectFolders (string path, ProjectResources projectResources)
        {
            return projectResources.ResourcePaths.LocateFolderPathsAtFolder(path).Select(p => new Folder(p)).ToList();
        }
    }
}
