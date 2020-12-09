using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityCommon
{
    public class LocalResourceProvider : ResourceProvider
    {
        public readonly string RootPath;

        private readonly Dictionary<Type, IConverter> converters = new Dictionary<Type, IConverter>();

        /// <param name="rootPath">
        /// An absolute path to the folder where the resources are located,
        /// or a relative path with one of the available origins:
        /// - %DATA% - <see cref="Application.dataPath"/>
        /// - %PDATA% - <see cref="Application.persistentDataPath"/>
        /// - %STREAM% - <see cref="Application.streamingAssetsPath"/>
        /// - %SPECIAL{F}%, where F is a value from <see cref="Environment.SpecialFolder"/> - <see cref="Environment.GetFolderPath(Environment.SpecialFolder)"/>
        /// </param>
        public LocalResourceProvider (string rootPath)
        {
            if (rootPath.StartsWith("%DATA%")) RootPath = string.Concat(Application.dataPath, rootPath.GetAfterFirst("%DATA%"));
            else if (rootPath.StartsWith("%PDATA%")) RootPath = string.Concat(Application.persistentDataPath, rootPath.GetAfterFirst("%PDATA%"));
            else if (rootPath.StartsWith("%STREAM%")) RootPath = string.Concat(Application.streamingAssetsPath, rootPath.GetAfterFirst("%STREAM%"));
            else if (rootPath.StartsWith("%SPECIAL{"))
            {
                var specialFolderStr = rootPath.GetBetween("%SPECIAL{", "}%");
                if (!Enum.TryParse<Environment.SpecialFolder>(specialFolderStr, true, out var specialFolder))
                    throw new Exception($"Failed to parse `{rootPath}` special folder path for local resource provider root.");
                RootPath = string.Concat(Environment.GetFolderPath(specialFolder), rootPath.GetAfterFirst("}%"));
            }
            else RootPath = rootPath; // Absolute path.

            RootPath = RootPath.Replace("\\", "/");
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

        protected override LocateFoldersRunner CreateLocateFoldersRunner (string path)
        {
            return new LocalFolderLocator(this, RootPath, path);
        }

        protected override void DisposeResource (Resource resource)
        {
            if (!resource.Valid) return;
            ObjectUtils.DestroyOrImmediate(resource.Object);
        }

        private IRawConverter<T> ResolveConverter<T> ()
        {
            var resourceType = typeof(T);
            if (!converters.ContainsKey(resourceType))
                throw new Exception($"Converter for resource of type '{resourceType.Name}' is not available.");
            return converters[resourceType] as IRawConverter<T>;
        }
    }
}
