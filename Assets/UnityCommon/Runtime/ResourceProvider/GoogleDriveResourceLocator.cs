#if UNITY_GOOGLE_DRIVE_AVAILABLE

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityGoogleDrive;

namespace UnityCommon
{
    public class GoogleDriveResourceLocator<TResource> : LocateResourcesRunner<TResource> 
        where TResource : UnityEngine.Object
    {
        public readonly string RootPath;

        private IRawConverter<TResource> converter;

        public GoogleDriveResourceLocator (IResourceProvider provider, string rootPath, string resourcesPath, 
            IRawConverter<TResource> converter) : base (provider, resourcesPath)
        {
            RootPath = rootPath;
            this.converter = converter;
        }

        public override async Task RunAsync ()
        {
            var result = new List<string>();

            // 1. Find all the files by path.
            var fullpath = PathUtils.Combine(RootPath, Path) + "/";
            var files = await Helpers.FindFilesByPathAsync(fullpath, fields: new List<string> { "files(name, mimeType)" });

            // 2. Filter the results by represenations (MIME types).
            var reprToFileMap = new Dictionary<RawDataRepresentation, List<UnityGoogleDrive.Data.File>>();
            foreach (var representation in converter.Representations)
                reprToFileMap.Add(representation, files.Where(f => f.MimeType == representation.MimeType).ToList());

            // 3. Create resources using located files.
            foreach (var reprToFile in reprToFileMap)
            {
                foreach (var file in reprToFile.Value)
                {
                    var fileName = string.IsNullOrEmpty(reprToFile.Key.Extension) ? file.Name : file.Name.GetBeforeLast(".");
                    var filePath = string.IsNullOrEmpty(Path) ? fileName : string.Concat(Path, '/', fileName);
                    result.Add(filePath);
                }
            }

            SetResult(result);
        }
    }
}

#endif
