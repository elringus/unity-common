using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityCommon
{
    public class LocalResourceProvider : ResourceProvider
    {
        /// <summary>
        /// Path to the folder where resources are located (realtive to <see cref="Application.dataPath"/>).
        /// </summary>
        public readonly string RootPath;

        private readonly Dictionary<Type, IConverter> converters = new Dictionary<Type, IConverter>();

        public LocalResourceProvider (string rootPath)
        {
            RootPath = rootPath;
        }

        public override bool SupportsType<T> () => converters.ContainsKey(typeof(T));

        /// <summary>
        /// Adds a resource type converter.
        /// </summary>
        public void AddConverter<T> (IRawConverter<T> converter)
        {
            if (converters.ContainsKey(typeof(T))) return;
            converters.Add(typeof(T), converter);
        }

        protected override LoadResourceRunner<T> CreateLoadResourceRunner<T> (string path)
        {
            return new LocalResourceLoader<T>(this, RootPath, path, ResolveConverter<T>(), LogMessage);
        }

        protected override LocateResourcesRunner<T> CreateLocateResourcesRunner<T> (string path)
        {
            return new LocalResourceLocator<T>(this, RootPath, path, ResolveConverter<T>());
        }

        protected override Task UnloadResourceAsync (Resource resource)
        {
            UnloadResourceBlocking(resource);
            return Task.CompletedTask;
        }

        protected override IEnumerable<Folder> LocateFoldersBlocking (string path)
        {
            return LocalFolderLocator.LocateFoldersAtPath(RootPath, path);
        }

        protected override LocateFoldersRunner CreateLocateFoldersRunner (string path)
        {
            return new LocalFolderLocator(this, RootPath, path);
        }

        protected override Resource<T> LoadResourceBlocking<T> (string path)
        {
            var converter = ResolveConverter<T>();
            var rawData = default(byte[]);
            var startTime = Time.time;

            var filePath = string.IsNullOrEmpty(RootPath) ? path : string.Concat(RootPath, '/', path);
            filePath = string.Concat(Application.dataPath, "/", filePath);

            foreach (var representation in converter.Representations)
            {
                var fullPath = string.Concat(filePath, representation.Extension);
                if (!File.Exists(fullPath)) continue;

                rawData = File.ReadAllBytes(filePath);
                break;
            }

            if (rawData is null)
            {
                var usedExtensions = string.Join("/", converter.Representations.Select(r => r.Extension));
                Debug.LogError($"Failed to load `{filePath}({usedExtensions})` resource using local file system: File not found.");
                return new Resource<T>(path, null, this);
            }

            LogMessage($"Resource `{path}` loaded {StringUtils.FormatFileSize(rawData.Length)} over {Time.time - startTime:0.###} seconds.");

            var obj = converter.Convert(rawData);
            return new Resource<T>(path, obj, this);
        }

        protected override IEnumerable<string> LocateResourcesBlocking<T> (string path)
        {
            return LocalResourceLocator<T>.LocateResources(RootPath, path, ResolveConverter<T>());
        }

        protected override void UnloadResourceBlocking (Resource resource)
        {
            if (!resource.IsValid) return;
            ObjectUtils.DestroyOrImmediate(resource.Object);
        }

        private IRawConverter<T> ResolveConverter<T> ()
        {
            var resourceType = typeof(T);
            if (!converters.ContainsKey(resourceType))
            {
                Debug.LogError($"Converter for resource of type '{resourceType.Name}' is not available.");
                return null;
            }
            return converters[resourceType] as IRawConverter<T>;
        }
    }
}
