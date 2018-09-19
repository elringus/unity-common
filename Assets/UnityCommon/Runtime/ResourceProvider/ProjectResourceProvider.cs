using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        protected override Task UnloadResourceAsync (Resource resource)
        {
            if (resource.IsValid && resource.IsUnloadable)
                UnityEngine.Resources.UnloadAsset(resource.AsUnityObject);
            return Task.CompletedTask;
        }
    }
}
