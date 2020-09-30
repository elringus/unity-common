using System;
using UnityEngine;

namespace UnityCommon
{
    /// <summary>
    /// Represents a <see cref="UnityEngine.Object"/> associated with a string identifier (path).
    /// </summary>
    public class Resource
    {
        /// <summary>
        /// A cached invalid resource.
        /// </summary>
        public static readonly Resource Invalid = new Resource(null, null);

        /// <summary>
        /// Full path to the resource location; also serves as an ID within the provider.
        /// </summary>
        public readonly string Path;
        /// <summary>
        /// Actual object (data) represented by the resource.
        /// </summary>
        public readonly UnityEngine.Object Object;
        /// <summary>
        /// Whether <see cref="Object"/> is a valid (not-destroyed) instance.
        /// </summary>
        public bool Valid => ObjectUtils.IsValid(Object);

        public Resource (string path, UnityEngine.Object obj)
        {
            Path = path;
            Object = obj;
        }

        public static implicit operator UnityEngine.Object (Resource resource) => resource?.Object;

        public override string ToString () => $"Resource<{(Valid ? Object.GetType().Name : "INVALID")}>@{Path}";
    }
    
    /// <summary>
    /// Represents a strongly typed <see cref="UnityEngine.Object"/> associated with a string identifier (path).
    /// </summary>
    /// <typeparam name="TResource">Type of the resource object.</typeparam>
    public class Resource<TResource> : Resource
        where TResource : UnityEngine.Object
    {
        /// <summary>
        /// A cached invalid resource.
        /// </summary>
        public new static readonly Resource<TResource> Invalid = new Resource<TResource>(null, null);
        
        /// <summary>
        /// Actual object (data) represented by the resource.
        /// </summary>
        public new TResource Object => CastObject(base.Object);

        public Resource (string path, TResource obj) 
            : base(path, obj) { }

        public static implicit operator TResource (Resource<TResource> resource) => resource?.Object;

        private TResource CastObject (UnityEngine.Object obj)
        {
            if (!Valid) return null;

            var castedObj = obj as TResource;
            if (castedObj is null)
                throw new Exception($"Resource `{Path}` is not of type `{typeof(TResource).FullName}`.");

            return castedObj;
        }
    }
}
