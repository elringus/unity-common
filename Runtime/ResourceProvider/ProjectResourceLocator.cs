using System.Collections.Generic;
using UniRx.Async;

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

        public override UniTask RunAsync ()
        {
            var locatedResourcePaths = LocateProjectResources(Path, projectResources);
            SetResult(locatedResourcePaths);
            return UniTask.CompletedTask;
        }

        public static IEnumerable<string> LocateProjectResources (string path, ProjectResources projectResources)
        {
            return projectResources.ResourcePaths.LocateResourcePathsAtFolder(path);
        }
    }
}
