using UnityEngine;

namespace UnityCommon
{
    /// <summary>
    /// Represents a <see cref="UnityEngine.Object"/> stored at the specified path. 
    /// </summary>
    [System.Serializable]
    public class Resource
    {
        public string Path { get => path; private set => path = value; }
        public Object Object { get => obj; set => obj = value; }
        public bool IsValid => Object != null && Object;

        [SerializeField] private string path;
        [SerializeField] private Object obj;

        // For serialization to work properly.
        public Resource () : this(null) { }

        public Resource (string path, Object obj = default)
        {
            Path = path;
            Object = obj;
        }

        public static implicit operator Object (Resource resource) => resource.Object;
    }

    /// <summary>
    /// Represents a strongly typed <see cref="UnityEngine.Object"/> stored at the specified path. 
    /// </summary>
    /// <typeparam name="T">Type of the resource object.</typeparam>
    public class Resource<T> : Resource where T : Object
    {
        public new T Object { get => CastObject(base.Object); set => base.Object = value; }

        public Resource (string path, T obj = default) : base(path, obj) { }

        public static implicit operator T (Resource<T> resource) => resource?.Object;

        private T CastObject (Object obj)
        {
            if (!IsValid) return null;

            var castedObj = obj as T;
            if (castedObj is null)
            {
                Debug.LogError($"Resource '{Path}' is not of type '{typeof(T).FullName}'.");
                return null;
            }

            return castedObj;
        }
    }
}
