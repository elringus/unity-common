using System;
using System.Collections.Generic;
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
        private Dictionary<Type, TypeRedirector> redirectors;

        public ProjectResourceProvider ()
        {
            projectResources = ProjectResources.Get();
            redirectors = new Dictionary<Type, TypeRedirector>();
        }

        public override bool SupportsType<T> () => true;

        public void AddRedirector<TSource, TRedirect> (IConverter<TRedirect, TSource> redirectToSourceConverter)
        {
            var sourceType = typeof(TSource);
            if (!redirectors.ContainsKey(sourceType))
            {
                var redirector = new TypeRedirector(sourceType, typeof(TRedirect), redirectToSourceConverter);
                redirectors.Add(redirector.SourceType, redirector);
            }
        }

        protected override LoadResourceRunner<T> CreateLoadResourceRunner<T> (Resource<T> resource)
        {
            return new ProjectResourceLoader<T>(resource, redirectors.ContainsKey(typeof(T)) ? redirectors[typeof(T)] : null, LogMessage);
        }

        protected override LocateResourcesRunner<T> CreateLocateResourcesRunner<T> (string path)
        {
            return new ProjectResourceLocator<T>(path, projectResources);
        }

        protected override void UnloadResourceBlocking (Resource resource)
        {
            if (!resource.IsValid) return;

            // Non-asset resources are created when using type redirectors.
            if (redirectors.Count > 0 && redirectors.ContainsKey(resource.Object.GetType()))
            {
                Debug.Log(resource.Path);
                if (!Application.isPlaying) UnityEngine.Object.DestroyImmediate(resource.Object);
                else UnityEngine.Object.Destroy(resource.Object);
                return;
            }

            // Can't unload prefabs: https://forum.unity.com/threads/393385.
            // TODO: Replace the project provider with addressable system in Unity 2019?
            if (resource.Object is GameObject) return;

            Resources.UnloadAsset(resource.Object);
        }

        protected override Task UnloadResourceAsync (Resource resource)
        {
            // Unity doesn't provide async unload API.
            UnloadResourceBlocking(resource);
            return Task.CompletedTask;
        }

        protected override Resource<T> LoadResourceBlocking<T> (string path)
        {
            var resource = new Resource<T>(path);
            var redirector = redirectors.ContainsKey(typeof(T)) ? redirectors[typeof(T)] : null;

            var resourceType = redirector != null ? redirector.RedirectType : typeof(T);
            var obj = Resources.Load(resource.Path, resourceType);
            resource.Object = redirector != null ? redirector.ToSource<T>(obj) : obj as T;
            return resource;
        }

        protected override IEnumerable<Resource<T>> LocateResourcesBlocking<T> (string path)
        {
            return ProjectResourceLocator<T>.LocateProjectResources(path, projectResources);
        }

        protected override IEnumerable<Folder> LocateFoldersBlocking (string path)
        {
            return ProjectFolderLocator.LocateProjectFolders(path, projectResources);
        }

        protected override LocateFoldersRunner CreateLocateFoldersRunner (string path)
        {
            return new ProjectFolderLocator(path, projectResources);
        }
    }
}
