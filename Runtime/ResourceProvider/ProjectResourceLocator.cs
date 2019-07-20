using System.Collections.Generic;
using System.Threading.Tasks;

namespace UnityCommon
{
    public class ProjectResourceLocator<TResource> : LocateResourcesRunner<TResource> 
        where TResource : UnityEngine.Object
    {
        private readonly ProjectResources projectResources;

        public ProjectResourceLocator (IResourceProvider provider, string resourcesPath, 
            ProjectResources projectResources) : base (provider, resourcesPath ?? string.Empty)
        {
            this.projectResources = projectResources;
        }

        public override Task RunAsync ()
        {
            var locatedResourcePaths = LocateProjectResources(Path, projectResources);
            SetResult(locatedResourcePaths);
            return Task.CompletedTask;
        }

        public static IEnumerable<string> LocateProjectResources (string path, ProjectResources projectResources)
        {
            return projectResources.ResourcePaths.LocateResourcePathsAtFolder(path);
        }
    }
}
