using System;
using System.Collections.Generic;
using System.Linq;
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
        /// A cached invalid resource.
        /// </summary>
        public static readonly Resource Invalid = new Resource(null, null, null);

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
        public bool Valid => ObjectUtils.IsValid(Object);
        /// <summary>
        /// How many objects are currently holding (using) the resource.
        /// </summary>
        public int HoldersCount => holders.Count;

        private readonly LinkedList<WeakReference> holders = new LinkedList<WeakReference>();

        public Resource (string path, UnityEngine.Object obj, IResourceProvider provider)
        {
            Path = path;
            Object = obj;
            Provider = provider;
        }

        public static implicit operator UnityEngine.Object (Resource resource) => resource?.Object;

        public override string ToString () => $"Resource<{(ObjectUtils.IsValid(Object) ? Object.GetType() : default)}> {Path}@{Provider.GetType().Name}";

        /// <summary>
        /// Registers the provided object as a holder of the resource.
        /// The resource won't be unloaded by <see cref="Release(object, bool)"/> while it's held by at least one object.
        /// </summary>
        /// <param name="holder">The object that is going to hold the resource.</param>
        public void Hold (object holder)
        {
            if (IsHeldBy(holder)) return;
            var holderRef = new WeakReference(holder);
            holders.AddLast(holderRef);
        }

        /// <summary>
        /// Removes the provided object from the holders list.
        /// Will (optionally) unload the resource after the release in case no other objects are holding it.
        /// </summary>
        /// <param name="holder">The object that is no longer holding the resource.</param>
        /// <param name="unload">Whether to also unload the resource in case no other objects are holding it.</param>
        public void Release (object holder, bool unload = true)
        {
            holders.RemoveAll(wr => !wr.IsAlive || wr.Target == holder);
            if (unload && holders.Count == 0)
                Provider.UnloadResource(Path);
        }

        /// <summary>
        /// Checks whether the resource is being held by the provided object.
        /// </summary>
        public bool IsHeldBy (object holder) => holders.Any(wr => wr.Target == holder);
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
            if (!Valid) return null;

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
