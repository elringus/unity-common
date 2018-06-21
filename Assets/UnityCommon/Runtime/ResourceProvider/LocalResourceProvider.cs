using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityCommon
{
    public class LocalResourceProvider : MonoRunnerResourceProvider
    {
        /// <summary>
        /// Path to the folder where resources are located (realtive to <see cref="Application.dataPath"/>).
        /// </summary>
        public string RootPath { get; set; }

        private Dictionary<Type, IConverter> converters = new Dictionary<Type, IConverter>();

        /// <summary>
        /// Adds a resource type converter.
        /// </summary>
        public void AddConverter<T> (IRawConverter<T> converter) where T : class
        {
            if (converters.ContainsKey(typeof(T))) return;
            converters.Add(typeof(T), converter);
        }

        protected override LoadResourceRunner<T> CreateLoadRunner<T> (Resource<T> resource)
        {
            return new LocalResourceLoader<T>(RootPath, resource, ResolveConverter<T>(), LogMessage);
        }

        protected override LocateResourcesRunner<T> CreateLocateRunner<T> (string path)
        {
            return new LocalResourceLocator<T>(RootPath, path, ResolveConverter<T>());
        }

        protected override void UnloadResource (Resource resource)
        {
            if (resource.IsValid && resource.IsUnityObject)
                Destroy(resource.AsUnityObject);
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
