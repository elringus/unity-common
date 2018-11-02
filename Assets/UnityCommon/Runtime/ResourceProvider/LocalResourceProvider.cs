using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityCommon
{
    public class LocalResourceProvider : ResourceProvider
    {
        /// <summary>
        /// Path to the folder where resources are located (realtive to <see cref="Application.dataPath"/>).
        /// </summary>
        public string RootPath { get; private set; }

        private Dictionary<Type, IConverter> converters = new Dictionary<Type, IConverter>();

        public LocalResourceProvider (string rootPath)
        {
            RootPath = rootPath;
        }

        /// <summary>
        /// Adds a resource type converter.
        /// </summary>
        public void AddConverter<T> (IRawConverter<T> converter)
        {
            if (converters.ContainsKey(typeof(T))) return;
            converters.Add(typeof(T), converter);
        }

        protected override LoadResourceRunner<T> CreateLoadResourceRunner<T> (Resource<T> resource)
        {
            return new LocalResourceLoader<T>(RootPath, resource, ResolveConverter<T>(), LogMessage);
        }

        protected override LocateResourcesRunner<T> CreateLocateResourcesRunner<T> (string path)
        {
            return new LocalResourceLocator<T>(RootPath, path, ResolveConverter<T>());
        }

        protected override Task UnloadResourceAsync (Resource resource)
        {
            UnloadResourceBlocking(resource);
            return Task.CompletedTask;
        }

        // TODO: Support blocking mode (?).
        protected override Resource<T> LoadResourceBlocking<T> (string path) { throw new NotImplementedException(); }
        protected override IEnumerable<Resource<T>> LocateResourcesBlocking<T> (string path) { throw new NotImplementedException(); }

        protected override void UnloadResourceBlocking (Resource resource)
        {
            if (resource.IsValid)
            {
                if (!Application.isPlaying) UnityEngine.Object.DestroyImmediate(resource.Object);
                else UnityEngine.Object.Destroy(resource.Object);
            }
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
