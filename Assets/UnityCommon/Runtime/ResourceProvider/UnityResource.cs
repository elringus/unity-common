using UnityEngine;

public class UnityResource
{
    public string Path { get; private set; }
    public Object Object { get; set; }
    public bool IsValid { get { return Object; } }

    public UnityResource (string path)
    {
        Path = path;
    }
}

public class UnityResource<T> : UnityResource where T : Object
{
    public new T Object { get { return CastObject(base.Object); } set { Object = value; } }

    public UnityResource (string path) : base(path) { }

    private T CastObject (Object resourceObject)
    {
        var castedResource = resourceObject as T;
        if (!castedResource)
        {
            Debug.LogError(string.Format("Resource '{0}' is not of type '{1}'.", Path, typeof(T).Name));
            return null;
        }
        return castedResource;
    }
}
