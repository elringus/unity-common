using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityCommon
{
    /// <summary>
    /// Represents a <see cref="UnityEngine.Object"/> associated with a 
    /// specific path and loaded by a <see cref="IResourceProvider"/>. 
    /// </summary>
    public class Resource
    {
        /// <summary>
        /// Full path to the resource location; also serves as an ID within the provider.
        /// </summary>
        public readonly string Path;
        /// <summary>
        /// Actual object (data) represented by the resource.
        /// </summary>
        public readonly UnityEngine.Object Object;
        /// <summary>
        /// Provider that was used to load the resource.
        /// </summary>
        public readonly IResourceProvider Provider;
        /// <summary>
        /// Whether the <see cref="Object"/> is currently alive (not destroyed) 
        /// on both managed and unmanaged sides of the engine.
        /// </summary>
        public bool IsValid => ObjectUtils.IsValid(Object);

        private readonly HashSet<WeakReference> holders = new HashSet<WeakReference>();

        public Resource (string path, UnityEngine.Object obj, IResourceProvider provider)
        {
            Path = path;
            Object = obj;
            Provider = provider;
        }

        public static implicit operator UnityEngine.Object (Resource resource) => resource?.Object;

        public override string ToString () => $"Resource<{Object?.GetType()}> {Path}@{Provider.GetType().Name}";

        /// <summary>
        /// Registers the provided object as a holder of the resource.
        /// The resource won't be unloaded by <see cref="Release(object)"/> while it's held by at least one object.
        /// </summary>
        public void Hold (object holder)
        {
            var holderRef = new WeakReference(holder);
            holders.Add(holderRef);
        }

        /// <summary>
        /// Removes the provided object from the holders set.
        /// Will also unload the resource in case no object is currently holding it.
        /// </summary>
        public void Release (object holder)
        {
            holders.RemoveWhere(hr => !hr.IsAlive || hr.Target == holder);
            if (holders.Count == 0)
                Provider.UnloadResource(Path);
        }
    }

    /// <summary>
    /// Represents a strongly typed <see cref="UnityEngine.Object"/> associated 
    /// with a specific path and loaded by a <see cref="IResourceProvider"/>. 
    /// </summary>
    /// <typeparam name="TResource">Type of the resource object.</typeparam>
    public class Resource<TResource> : Resource where TResource : UnityEngine.Object
    {
        /// <summary>
        /// Actual object (data) represented by the resource.
        /// </summary>
        public new TResource Object => CastObject(base.Object);

        public Resource (string path, TResource obj, IResourceProvider provider) 
            : base(path, obj, provider) { }

        public static implicit operator TResource (Resource<TResource> resource) => resource?.Object;

        private TResource CastObject (UnityEngine.Object obj)
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
