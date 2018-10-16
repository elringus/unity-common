using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityCommon
{
    /// <summary>
    /// Provides resources stored in the 'Resources' folders of the project.
    /// </summary>
    public class ProjectResourceProvider : ResourceProvider
    {
        public class TypeRedirector
        {
            public Type SourceType { get; private set; }
            public Type RedirectType { get; private set; }
            public IConverter RedirectToSourceConverter { get; private set; }

            public TypeRedirector (Type sourceType, Type redirectType, IConverter redirectToSourceConverter)
            {
                SourceType = sourceType;
                RedirectType = redirectType;
                RedirectToSourceConverter = redirectToSourceConverter;
            }

            public async Task<TSource> ToSourceAsync<TSource> (object obj)
            {
                return (TSource)await RedirectToSourceConverter.ConvertAsync(obj);
            }

            public TSource ToSource<TSource> (object obj)
            {
                return (TSource)RedirectToSourceConverter.Convert(obj);
            }
        }

        private ProjectResources projectResources;
        private Dictionary<Type, TypeRedirector> redirectors = new Dictionary<Type, TypeRedirector>();

        public ProjectResourceProvider ()
        {
            projectResources = ProjectResources.Get();
        }

        public void AddRedirector<TSource, TRedirect> (IConverter<TRedirect, TSource> redirectToSourceConverter)
        {
            var sourceType = typeof(TSource);
            if (!redirectors.ContainsKey(sourceType))
            {
                var redirector = new TypeRedirector(sourceType, typeof(TRedirect), redirectToSourceConverter);
                redirectors.Add(redirector.SourceType, redirector);
            }
        }

        protected override LoadResourceRunner<T> CreateLoadRunner<T> (Resource<T> resource)
        {
            return new ProjectResourceLoader<T>(resource, redirectors.ContainsKey(typeof(T)) ? redirectors[typeof(T)] : null, LogMessage);
        }

        protected override LocateResourcesRunner<T> CreateLocateRunner<T> (string path)
        {
            return new ProjectResourceLocator<T>(path, projectResources, redirectors.ContainsKey(typeof(T)) ? redirectors[typeof(T)] : null);
        }

        protected override void UnloadResourceBlocking (Resource resource)
        {
            if (resource.IsValid && resource.IsUnloadable)
                UnityEngine.Resources.UnloadAsset(resource.AsUnityObject);
        }

        protected override Task UnloadResourceAsync (Resource resource)
        {
            // TODO: Support async unloading (?).
            UnloadResourceBlocking(resource);
            return Task.CompletedTask;
        }

        protected override Resource<T> LoadResourceBlocking<T> (string path)
        {
            var resource = new Resource<T>(path);
            var redirector = redirectors.ContainsKey(typeof(T)) ? redirectors[typeof(T)] : null;

            // Corner case when loading folders.
            if (typeof(T) == typeof(Folder))
            {
                (resource as Resource<Folder>).Object = new Folder(resource.Path);
                return resource;
            }

            var resourceType = redirector != null ? redirector.RedirectType : typeof(T);
            var obj = Resources.Load(resource.Path, resourceType);
            resource.Object = redirector != null ? redirector.ToSource<T>(obj) : (T)(object)obj;
            return resource;
        }

        protected override IEnumerable<Resource<T>> LocateResourcesBlocking<T> (string path)
        {
            var locatedResources = new List<Resource<T>>();
            var redirector = redirectors.ContainsKey(typeof(T)) ? redirectors[typeof(T)] : null;

            // Corner case when locating folders (unity doesn't see folder as a resource).
            if (typeof(T) == typeof(Folder))
            {
                return projectResources.LocateAllResourceFolders().FindAllAtPath(path)
                    .Select(f => new Resource<Folder>(f.Path, f) as Resource<T>).ToList();
            }

            var redirectType = redirector != null ? redirector.RedirectType : typeof(T);
            var objects = Resources.LoadAll(path, redirectType);

            foreach (var obj in objects)
            {
                var objPath = string.Concat(path, "/", obj.name);
                var cObj = redirector != null ? redirector.ToSource<T>(obj) : (T)(object)obj;
                var resource = new Resource<T>(objPath, cObj);
                locatedResources.Add(resource);
            }

            return locatedResources;
        }
    }
}
