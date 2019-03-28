using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UnityCommon
{
    public class EditorFolderLocator : LocateFoldersRunner
    {
        public string ResourcesPath { get; private set; }

        private IEnumerable<string> editorResourcePaths;

        public EditorFolderLocator (string resourcesPath, IEnumerable<string> editorResourcePaths)
        {
            ResourcesPath = resourcesPath ?? string.Empty;
            this.editorResourcePaths = editorResourcePaths;
        }

        public override async Task Run ()
        {
            await base.Run();

            LocatedFolders = LocateEditorFolders(ResourcesPath, editorResourcePaths);

            HandleOnCompleted();
        }

        public static List<Folder> LocateEditorFolders (string path, IEnumerable<string> editorResourcePaths)
        {
            return editorResourcePaths.LocateFolderPathsAtFolder(path).Select(p => new Folder(p)).ToList();
        }
    }
}
