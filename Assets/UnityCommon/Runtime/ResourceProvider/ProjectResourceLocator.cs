using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityCommon
{
    public class ProjectResourceLocator<TResource> : LocateResourcesRunner<TResource> where TResource : class
    {
        public string ResourcesPath { get; private set; }

        private ProjectResourceProvider.TypeRedirector redirector;
        private ProjectResources resources;

        public ProjectResourceLocator (string resourcesPath, ProjectResources resources, ProjectResourceProvider.TypeRedirector redirector = null)
        {
            ResourcesPath = resourcesPath ?? string.Empty;
            this.resources = resources;
            this.redirector = redirector;
        }

        public override async Task Run ()
        {
            await base.Run();

            // Corner case when locating folders (unity doesn't see folder as a resource).
            if (typeof(TResource) == typeof(Folder))
            {
                LocatedResources = resources.LocateAllResourceFolders().FindAllAtPath(ResourcesPath)
                    .Select(f => new Resource<Folder>(f.Path, f) as Resource<TResource>).ToList();
                HandleOnCompleted();
                return;
            }

            // TODO: Make this async (if possible, LoadAllAsync doesn't exist).
            var redirectType = redirector != null ? redirector.RedirectType : typeof(TResource);
            var objects = Resources.LoadAll(ResourcesPath, redirectType);

            foreach (var obj in objects)
            {
                var path = string.Concat(ResourcesPath, "/", obj.name);
                var cObj = redirector != null ? await redirector.ToSourceAsync<TResource>(obj) : obj as TResource;
                var resource = new Resource<TResource>(path, cObj);
                LocatedResources.Add(resource);
            }

            HandleOnCompleted();
        }
    }
}
