using System.Collections.Generic;
using System.Linq;
using UniRx.Async;

namespace UnityCommon
{
    public class ProjectResourceLocator<TResource> : LocateResourcesRunner<TResource> 
        where TResource : UnityEngine.Object
    {
        public readonly string RootPath;

        private readonly ProjectResources projectResources;

        public ProjectResourceLocator (IResourceProvider provider, string rootPath, string resourcesPath, 
            ProjectResources projectResources) : base (provider, resourcesPath ?? string.Empty)
        {
            RootPath = rootPath;
            this.projectResources = projectResources;
        }

        public override UniTask RunAsync ()
        {
            var locatedResourcePaths = LocateProjectResources(RootPath, Path, projectResources);
            SetResult(locatedResourcePaths);
            return UniTask.CompletedTask;
        }

        public static IEnumerable<string> LocateProjectResources (string rootPath, string resourcesPath, ProjectResources projectResources)
        {
            var path = string.IsNullOrEmpty(rootPath) ? resourcesPath : string.IsNullOrEmpty(resourcesPath) ? rootPath : $"{rootPath}/{resourcesPath}";
            var result = projectResources.ResourcePaths.LocateResourcePathsAtFolder(path);
            if (!string.IsNullOrEmpty(rootPath))
                return result.Select(p => p.GetAfterFirst(rootPath + "/"));
            return result;
        }
    }
}
