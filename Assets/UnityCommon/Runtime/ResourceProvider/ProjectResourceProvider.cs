using System;
using System.Collections.Generic;
using UniRx.Async;
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
            public Type SourceType { get; }
            public Type RedirectType { get; }
            public IConverter RedirectToSourceConverter { get; }

            public TypeRedirector (Type sourceType, Type redirectType, IConverter redirectToSourceConverter)
            {
                SourceType = sourceType;
                RedirectType = redirectType;
                RedirectToSourceConverter = redirectToSourceConverter;
            }

            public async UniTask<TSource> ToSourceAsync<TSource> (object obj, string name)
            {
                return (TSource)await RedirectToSourceConverter.ConvertAsync(obj, name);
            }

            public TSource ToSource<TSource> (object obj, string name)
            {
                return (TSource)RedirectToSourceConverter.Convert(obj, name);
            }
        }

        public readonly string RootPath;

        private readonly ProjectResources projectResources;
        private readonly Dictionary<Type, TypeRedirector> redirectors;

        public ProjectResourceProvider (string rootPath = null)
        {
            projectResources = ProjectResources.Get();
            redirectors = new Dictionary<Type, TypeRedirector>();
            RootPath = rootPath;
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

        protected override LoadResourceRunner<T> CreateLoadResourceRunner<T> (string path)
        {
            return new ProjectResourceLoader<T>(this, RootPath, path, redirectors.ContainsKey(typeof(T)) ? redirectors[typeof(T)] : null, LogMessage);
        }

        protected override LocateResourcesRunner<T> CreateLocateResourcesRunner<T> (string path)
        {
            return new ProjectResourceLocator<T>(this, RootPath, path, projectResources);
        }

        protected override LocateFoldersRunner CreateLocateFoldersRunner (string path)
        {
            return new ProjectFolderLocator(this, RootPath, path, projectResources);
        }

        protected override void DisposeResource (Resource resource)
        {
            if (!resource.Valid) return;

            // Non-asset resources are created when using type redirectors.
            if (redirectors.Count > 0 && redirectors.ContainsKey(resource.Object.GetType()))
            {
                ObjectUtils.DestroyOrImmediate(resource.Object);
                return;
            }

            // Can't unload prefabs: https://forum.unity.com/threads/393385.
            if (resource.Object is GameObject || resource.Object is Component) return;

            Resources.UnloadAsset(resource.Object);
        }
    }
}
