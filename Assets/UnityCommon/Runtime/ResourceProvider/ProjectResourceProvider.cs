using System;
using System.Collections.Generic;
using System.Linq;
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

    private Dictionary<Type, TypeRedirector> redirectors = new Dictionary<Type, TypeRedirector>();

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

    protected override AsyncAction<List<Resource<T>>> LocateResourcesAtPath<T> (string path)
    {
        var sourceType = typeof(T);
        var redirectType = redirectors.ContainsKey(sourceType) ? redirectors[sourceType].RedirectType : sourceType;
        // TODO: Make this async (if possible, LoadAllAsync doesn't exist).
        var objects = UnityEngine.Resources.LoadAll(path, redirectType); 
        var resources = objects.Select(r => new Resource<T>(string.Concat(path, "/", r.name), 
            redirectors.ContainsKey(sourceType) ? redirectors[sourceType].ToSource<T>(r) : r as T));
        return new AsyncAction<List<Resource<T>>>(resources.ToList(), true);
    }

    protected override void UnloadResource (Resource resource)
    {
        if (resource.IsValid && resource.IsUnityObject)
            UnityEngine.Resources.UnloadAsset(resource.AsUnityObject);
    }
}
