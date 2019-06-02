using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

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

        public override Task RunAsync ()
        {
            var locatedFolders = LocateFoldersAtPath(RootPath, Path);
            SetResult(locatedFolders);
            return Task.CompletedTask;
        }

        public static List<Folder> LocateFoldersAtPath (string rootPath, string resourcesPath)
        {
            var locatedFolders = new List<Folder>();

            var folderPath = Application.dataPath;
            if (!string.IsNullOrEmpty(rootPath) && !string.IsNullOrEmpty(resourcesPath))
                folderPath += string.Concat('/', rootPath, '/', resourcesPath);
            else if (string.IsNullOrEmpty(rootPath)) folderPath += string.Concat('/', resourcesPath);
            else folderPath += string.Concat('/', rootPath);
            var parendFolder = new DirectoryInfo(folderPath);
            if (!parendFolder.Exists) return locatedFolders;

            foreach (var dir in parendFolder.GetDirectories())
            {
                var path = dir.FullName.Replace("\\", "/").GetAfterFirst(rootPath + "/");
                var folder = new Folder(path);
                locatedFolders.Add(folder);
            }

            return locatedFolders;
        }
    }
}
