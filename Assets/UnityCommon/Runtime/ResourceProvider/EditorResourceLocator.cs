using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UnityCommon
{
    public class EditorResourceLocator<TResource> : LocateResourcesRunner<TResource> where TResource : UnityEngine.Object
    {
        public string ResourcesPath { get; private set; }

        private IEnumerable<string> editorResourcePaths;

        public EditorResourceLocator (string resourcesPath, IEnumerable<string> editorResourcePaths)
        {
            ResourcesPath = resourcesPath ?? string.Empty;
            this.editorResourcePaths = editorResourcePaths;
        }

        public override async Task Run ()
        {
            await base.Run();

            LocatedResources = LocateProjectResources(ResourcesPath, editorResourcePaths);

            HandleOnCompleted();
        }

        public static List<Resource<TResource>> LocateProjectResources (string path, IEnumerable<string> editorResourcePaths)
        {
            return editorResourcePaths.LocateResourcePathsAtFolder(path)
                .Select(p => new Resource<TResource>(p)).ToList();
        }
    }

}
