using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UnityCommon
{
    public class ProjectResourceLocator<TResource> : LocateResourcesRunner<TResource> where TResource : UnityEngine.Object
    {
        public string ResourcesPath { get; private set; }

        private ProjectResources projectResources;

        public ProjectResourceLocator (string resourcesPath, ProjectResources projectResources)
        {
            ResourcesPath = resourcesPath ?? string.Empty;
            this.projectResources = projectResources;
        }

        public override async Task Run ()
        {
            await base.Run();

            LocatedResources = LocateProjectResources(ResourcesPath, projectResources);

            HandleOnCompleted();
        }

        public static List<Resource<TResource>> LocateProjectResources (string path, ProjectResources projectResources)
        {
            path = path ?? string.Empty;

            // Corner case when locating folders (Unity doesn't see folder as a resource).
            if (typeof(TResource) == typeof(Folder))
            {
                return projectResources.Folders.FindAllAtPath(path)
                    .Select(f => new Resource<Folder>(f.Path) as Resource<TResource>).ToList();
            }

            if (string.IsNullOrWhiteSpace(path))
                return projectResources.ResourcePaths.Where(p => !p.Contains("/") || string.IsNullOrEmpty(p.GetBeforeLast("/"))).Select(p => new Resource<TResource>(p)).ToList();
            return projectResources.ResourcePaths.Where(p => p.GetBeforeLast("/").Equals(path) || p.GetBeforeLast("/").Equals("/" + path)).Select(p => new Resource<TResource>(p)).ToList();
        }
    }
}
