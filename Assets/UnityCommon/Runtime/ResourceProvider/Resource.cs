using UnityEngine;

namespace UnityCommon
{
    /// <summary>
    /// Represents a <see cref="UnityEngine.Object"/> associated with a 
    /// specific path and managed by a <see cref="IResourceProvider"/>. 
    /// </summary>
    public class Resource
    {
        public readonly string Path;
        public readonly Object Object;
        public readonly IResourceProvider Provider;
        public bool IsValid => Object != null && Object;

        public Resource (string path, Object obj, IResourceProvider provider)
        {
            Path = path;
            Object = obj;
            Provider = provider;
        }

        public static implicit operator Object (Resource resource) => resource?.Object;

        public override string ToString () => $"Resource<{Object?.GetType()}> {Path}@{Provider.GetType().Name}";
    }

    /// <summary>
    /// Represents a strongly typed <see cref="UnityEngine.Object"/> associated 
    /// with a specific path and managed by a <see cref="IResourceProvider"/>. 
    /// </summary>
    /// <typeparam name="TResource">Type of the resource object.</typeparam>
    public class Resource<TResource> : Resource where TResource : Object
    {
        public new TResource Object => CastObject(base.Object);

        public Resource (string path, TResource obj, IResourceProvider provider) 
            : base(path, obj, provider) { }

        public static implicit operator TResource (Resource<TResource> resource) => resource?.Object;

        private TResource CastObject (Object obj)
        {
            if (!IsValid) return null;

            var castedObj = obj as TResource;
            if (castedObj is null)
            {
                Debug.LogError($"Resource '{Path}' is not of type '{typeof(TResource).FullName}'.");
                return null;
            }

            return castedObj;
        }
    }
}
