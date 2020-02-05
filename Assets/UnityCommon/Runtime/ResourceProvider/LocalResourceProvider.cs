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
        /// - %USER% - <see cref="Environment.SpecialFolder.UserProfile"/>
        /// </param>
        public LocalResourceProvider (string rootPath)
        {
            if (rootPath.StartsWith("%DATA%")) RootPath = string.Concat(Application.dataPath, rootPath.GetAfterFirst("%DATA%"));
            else if (rootPath.StartsWith("%PDATA%")) RootPath = string.Concat(Application.persistentDataPath, rootPath.GetAfterFirst("%PDATA%"));
            else if (rootPath.StartsWith("%STREAM%")) RootPath = string.Concat(Application.streamingAssetsPath, rootPath.GetAfterFirst("%STREAM%"));
            else if (rootPath.StartsWith("%USER%")) RootPath = string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), rootPath.GetAfterFirst("%USER%"));
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
