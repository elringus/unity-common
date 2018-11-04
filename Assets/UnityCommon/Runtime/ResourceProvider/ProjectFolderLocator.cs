using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UnityCommon
{
    public class ProjectFolderLocator : LocateFoldersRunner
    {
        public string ResourcesPath { get; private set; }

        private ProjectResources projectResources;

        public ProjectFolderLocator (string resourcesPath, ProjectResources projectResources)
        {
            ResourcesPath = resourcesPath ?? string.Empty;
            this.projectResources = projectResources;
        }

        public override async Task Run ()
        {
            await base.Run();

            LocatedFolders = LocateProjectFolders(ResourcesPath, projectResources);

            HandleOnCompleted();
        }

        public static List<Folder> LocateProjectFolders (string path, ProjectResources projectResources)
        {
            path = path ?? string.Empty;
            if (path.StartsWithFast("/")) path = path.GetAfterFirst("/") ?? string.Empty;
            if (!path.EndsWithFast("/")) path += "/";

            return projectResources.ResourcePaths
                .Where(p => p.StartsWithFast(path) && p.GetAfterFirst(path).Contains("/"))
                .Select(p => new Folder(path + p.GetBetween(path, "/")))
                .DistinctBy(f => f.Path).ToList();
        }
    }
}
