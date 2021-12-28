using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace UnityCommon
{
    public class LocalResourceLoader<TResource> : LoadResourceRunner<TResource>
        where TResource : UnityEngine.Object
    {
        public virtual string RootPath { get; }

        private readonly Action<string> logAction;
        private readonly IEnumerable<IRawConverter<TResource>> converters;
        private byte[] rawData;

        public LocalResourceLoader (IResourceProvider provider, string rootPath, string resourcePath,
            IEnumerable<IRawConverter<TResource>> converters, Action<string> logAction) : base(provider, resourcePath)
        {
            RootPath = rootPath;
            this.logAction = logAction;
            this.converters = converters;
        }

        public override async UniTask RunAsync ()
        {
            var startTime = Time.time;

            var filePath = string.Concat(RootPath, '/', Path);
            var selectedConverter = default(IRawConverter<TResource>);

            foreach (var converter in converters)
            foreach (var representation in converter.Representations)
            {
                var fullPath = string.Concat(filePath, representation.Extension);
                if (!File.Exists(fullPath)) continue;
                selectedConverter = converter;
                rawData = await IOUtils.ReadFileAsync(fullPath);
                break;
            }

            if (selectedConverter is null)
                throw new Error($"Failed to load `{filePath}` resource using local file system: failed to find compatible converter.");

            if (rawData is null)
            {
                var usedExtensions = string.Join("/", selectedConverter.Representations.Select(r => r.Extension));
                throw new Error($"Failed to load `{filePath}({usedExtensions})` resource using local file system: File not found.");
            }

            var obj = await selectedConverter.ConvertAsync(rawData, System.IO.Path.GetFileNameWithoutExtension(Path));
            var result = new Resource<TResource>(Path, obj);

            SetResult(result);

            logAction?.Invoke($"Resource `{Path}` loaded {StringUtils.FormatFileSize(rawData.Length)} over {Time.time - startTime:0.###} seconds.");
        }
    }
}
