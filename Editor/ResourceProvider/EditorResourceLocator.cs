using System.Collections.Generic;
using UniRx.Async;

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

        public override UniTask RunAsync ()
        {
            var locatedResourcePaths = LocateProjectResources(Path, editorResourcePaths);
            SetResult(locatedResourcePaths);
            return UniTask.CompletedTask;
        }

        public static IEnumerable<string> LocateProjectResources (string path, IEnumerable<string> editorResourcePaths)
        {
            return editorResourcePaths.LocateResourcePathsAtFolder(path);
        }
    }
}
