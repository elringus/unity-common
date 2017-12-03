using UnityEngine;

public class UnityResource
{
    public string Path { get; private set; }
    public virtual Object Object { get; private set; }
    public bool IsLoaded { get { return Object; } }

    public UnityResource (string path)
    {
        Path = path;
    }

    public virtual void ProvideLoadedObject (Object obj)
    {
        Object = obj;
    }
}

public class UnityResource<T> : UnityResource where T : Object
{
    public event System.Action<string, T> OnLoaded;

    public new T Object { get { return CastResource(base.Object); } }

    public UnityResource (string path) : base(path) { }

    private T CastResource (Object resourceObject)
    {
        var castedResource = resourceObject as T;
        if (!castedResource)
        {
            Debug.LogError(string.Format("Resource '{0}' is not of type '{1}'.", Path, typeof(T).Name));
            return null;
        }
        return castedResource;
    }

    public override void ProvideLoadedObject (Object obj)
    {
        base.ProvideLoadedObject(obj);
        OnLoaded.SafeInvoke(Path, Object);
    }
}
