using System;
using System.Collections.Generic;
using UnityEngine;

// StreamingAssets
public class LocalResourceProvider : IResourceProvider
{
    #pragma warning disable 67
    public event Action<float> OnLoadProgress;
    #pragma warning restore 67

    public bool IsLoading { get { throw new System.NotImplementedException(); } }
    public float LoadProgress { get { throw new System.NotImplementedException(); } }

    public AsyncAction<Resource<T>> LoadResource<T> (string path) where T : class
    {
        throw new System.NotImplementedException();
    }

    public AsyncAction<List<Resource<T>>> LoadResources<T> (string path) where T : class
    {
        throw new System.NotImplementedException();
    }

    public void UnloadResource (string path)
    {
        throw new System.NotImplementedException();
    }
}
