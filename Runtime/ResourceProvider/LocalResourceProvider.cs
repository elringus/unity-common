using System;
using System.Collections.Generic;
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
