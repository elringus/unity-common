using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UnityCommon
{
    public class EditorFolderLocator : LocateFoldersRunner
    {
        private readonly IEnumerable<string> editorResourcePaths;

        public EditorFolderLocator (IResourceProvider provider, string resourcesPath, IEnumerable<string> editorResourcePaths)
            : base (provider, resourcesPath ?? string.Empty)
        {
            this.editorResourcePaths = editorResourcePaths;
        }

        public override Task RunAsync ()
        {
            var locatedFolders = LocateEditorFolders(Path, editorResourcePaths);
            SetResult(locatedFolders);
            return Task.CompletedTask;
        }

        public static List<Folder> LocateEditorFolders (string path, IEnumerable<string> editorResourcePaths)
        {
            return editorResourcePaths.LocateFolderPathsAtFolder(path).Select(p => new Folder(p)).ToList();
        }
    }
}
