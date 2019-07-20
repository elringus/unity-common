#if UNITY_GOOGLE_DRIVE_AVAILABLE

using System.Collections.Generic;
using System.Threading.Tasks;
using UnityGoogleDrive;

namespace UnityCommon
{
    public class GoogleDriveFolderLocator : LocateFoldersRunner
    {
        public readonly string RootPath;

        public GoogleDriveFolderLocator (IResourceProvider provider, string rootPath, string resourcesPath)
            : base (provider, resourcesPath)
        {
            RootPath = rootPath;
        }

        public override async Task RunAsync ()
        {
            var result = new List<Folder>();

            var fullpath = PathUtils.Combine(RootPath, Path) + "/";
            var gFolders = await Helpers.FindFilesByPathAsync(fullpath, fields: new List<string> { "files(name)" }, mime: "application/vnd.google-apps.folder");

            foreach (var gFolder in gFolders)
            {
                var folderPath = string.IsNullOrEmpty(Path) ? gFolder.Name : string.Concat(Path, '/', gFolder.Name);
                var folder = new Folder(folderPath);
                result.Add(folder);
            }

            SetResult(result);
        }
    }
}

#endif
