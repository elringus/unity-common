using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Provides resources stored in the 'Resources' folders of the project.
/// </summary>
public class ProjectResourceProvider : MonoRunnerResourceProvider
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

        public TSource ToSource<TSource> (object obj) where TSource : class
        {
            return RedirectToSourceConverter.Convert(obj) as TSource;
        }
    }

    private ProjectResources projectResources;
    private Dictionary<Type, TypeRedirector> redirectors = new Dictionary<Type, TypeRedirector>();

    protected override void Awake ()
    {
        base.Awake();
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

    protected override AsyncRunner<Resource<T>> CreateLoadRunner<T> (Resource<T> resource)
    {
        return new ProjectResourceLoader<T>(resource, redirectors.ContainsKey(typeof(T)) ? redirectors[typeof(T)] : null, this);
    }

    protected override AsyncRunner<List<Resource<T>>> CreateLocateRunner<T> (string path)
    {
        return new ProjectResourceLocator<T>(path, projectResources, redirectors.ContainsKey(typeof(T)) ? redirectors[typeof(T)] : null, this);
    }

    protected override void UnloadResource (Resource resource)
    {
        if (resource.IsValid && resource.IsUnloadable)
            UnityEngine.Resources.UnloadAsset(resource.AsUnityObject);
    }
}
