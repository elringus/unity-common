using System.Collections.Generic;
using System.IO;
using UniRx.Async;

namespace UnityCommon
{
    public class LocalFolderLocator : LocateFoldersRunner
    {
        public readonly string RootPath;

        public LocalFolderLocator (IResourceProvider provider, string rootPath, string resourcesPath)
            : base (provider, resourcesPath)
        {
            RootPath = rootPath;
        }

        public override UniTask RunAsync ()
        {
            var locatedFolders = LocateFoldersAtPath(RootPath, Path);
            SetResult(locatedFolders);
            return UniTask.CompletedTask;
        }

        public static IReadOnlyCollection<Folder> LocateFoldersAtPath (string rootPath, string resourcesPath)
        {
            var locatedFolders = new List<Folder>();

            var folderPath = rootPath;
            if (!string.IsNullOrEmpty(resourcesPath))
                folderPath += string.Concat('/', resourcesPath);
            var parentFolder = new DirectoryInfo(folderPath);
            if (!parentFolder.Exists) return locatedFolders;

            foreach (var dir in parentFolder.GetDirectories())
            {
                var path = dir.FullName.Replace("\\", "/").GetAfterFirst(rootPath + "/");
                var folder = new Folder(path);
                locatedFolders.Add(folder);
            }

            return locatedFolders;
        }
    }
}
