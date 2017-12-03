using System;

public class RemoteResourceProvider : IResourceProvider
{
    #pragma warning disable 67
    public event Action<float> OnLoadProgress;
    #pragma warning restore 67

    public bool IsLoading { get { throw new System.NotImplementedException(); } }
    public float LoadProgress { get { throw new System.NotImplementedException(); } }

    public UnityResource<T> LoadResource<T> (string path) where T : UnityEngine.Object
    {
        throw new System.NotImplementedException();
    }

    public void UnloadResource (string path)
    {
        throw new System.NotImplementedException();
    }
}
