using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace UnityCommon
{
    public class LocalResourceLoader<TResource> : LoadResourceRunner<TResource> 
        where TResource : UnityEngine.Object
    {
        public virtual string RootPath { get; }

        private Action<string> logAction;
        private IRawConverter<TResource> converter;
        private byte[] rawData;

        public LocalResourceLoader (IResourceProvider provider, string rootPath, string resourcePath,
            IRawConverter<TResource> converter, Action<string> logAction) : base (provider, resourcePath)
        {
            RootPath = rootPath;
            this.logAction = logAction;
            this.converter = converter;
        }

        public override async UniTask RunAsync ()
        {
            var startTime = Time.time;

            var filePath = string.Concat(RootPath, '/', Path);

            foreach (var representation in converter.Representations)
            {
                var fullPath = string.Concat(filePath, representation.Extension);
                if (!File.Exists(fullPath)) continue;

                rawData = await IOUtils.ReadFileAsync(fullPath);
                break;
            }

            if (rawData is null)
            {
                var usedExtensions = string.Join("/", converter.Representations.Select(r => r.Extension));
                throw new Exception($"Failed to load `{filePath}({usedExtensions})` resource using local file system: File not found.");
            }

            var obj = await converter.ConvertAsync(rawData, System.IO.Path.GetFileNameWithoutExtension(Path));
            var result = new Resource<TResource>(Path, obj);

            SetResult(result);

            logAction?.Invoke($"Resource `{Path}` loaded {StringUtils.FormatFileSize(rawData.Length)} over {Time.time - startTime:0.###} seconds.");
        }
    }
}
