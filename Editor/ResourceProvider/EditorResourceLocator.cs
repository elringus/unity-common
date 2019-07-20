using System.Collections.Generic;
using System.Threading.Tasks;

namespace UnityCommon
{
    public class EditorResourceLocator<TResource> : LocateResourcesRunner<TResource> 
        where TResource : UnityEngine.Object
    {
        private readonly IEnumerable<string> editorResourcePaths;

        public EditorResourceLocator (IResourceProvider provider, string resourcesPath, 
            IEnumerable<string> editorResourcePaths) : base (provider, resourcesPath ?? string.Empty)
        {
            this.editorResourcePaths = editorResourcePaths;
        }

        public override Task RunAsync ()
        {
            var locatedResourcePaths = LocateProjectResources(Path, editorResourcePaths);
            SetResult(locatedResourcePaths);
            return Task.CompletedTask;
        }

        public static IEnumerable<string> LocateProjectResources (string path, IEnumerable<string> editorResourcePaths)
        {
            return editorResourcePaths.LocateResourcePathsAtFolder(path);
        }
    }
}
