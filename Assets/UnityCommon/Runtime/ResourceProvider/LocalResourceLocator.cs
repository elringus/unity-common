using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityCommon
{
    public class LocalResourceLocator<TResource> : LocateResourcesRunner<TResource> where TResource : UnityEngine.Object
    {
        public string RootPath { get; private set; }
        public string ResourcesPath { get; private set; }

        private IRawConverter<TResource> converter;

        public LocalResourceLocator (string rootPath, string resourcesPath, IRawConverter<TResource> converter)
        {
            RootPath = rootPath;
            ResourcesPath = resourcesPath;
            this.converter = converter;
        }

        public override async Task Run ()
        {
            await base.Run();

            LocatedResources = new List<Resource<TResource>>();

            // 1. Resolving parent folder.
            var folderPath = Application.dataPath;
            if (!string.IsNullOrEmpty(RootPath) && !string.IsNullOrEmpty(ResourcesPath))
                folderPath += string.Concat('/', RootPath, '/', ResourcesPath);
            else if (string.IsNullOrEmpty(RootPath)) folderPath += string.Concat('/', ResourcesPath);
            else folderPath += string.Concat('/', RootPath);
            var parendFolder = new DirectoryInfo(folderPath);
            if (!parendFolder.Exists)
            {
                HandleOnCompleted();
                return;
            }

            // 2. Searching for the files in the folder.
            var results = new Dictionary<RawDataRepresentation, List<FileInfo>>();
            foreach (var representation in converter.Representations.DistinctBy(r => r.Extension))
            {
                var files = parendFolder.GetFiles(string.Concat("*", representation.Extension)).ToList();
                if (files != null && files.Count > 0) results.Add(representation, files);
            }

            // 3. Create resources using located files.
            foreach (var result in results)
            {
                foreach (var file in result.Value)
                {
                    var fileName = string.IsNullOrEmpty(result.Key.Extension) ? file.Name : file.Name.GetBeforeLast(".");
                    var filePath = string.IsNullOrEmpty(ResourcesPath) ? fileName : string.Concat(ResourcesPath, '/', fileName);
                    var fileResource = new Resource<TResource>(filePath);
                    LocatedResources.Add(fileResource);
                }
            }

            HandleOnCompleted();
        }
    }

}
