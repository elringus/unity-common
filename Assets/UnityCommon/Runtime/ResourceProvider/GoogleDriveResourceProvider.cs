using System;
using UnityGoogleDrive;

/// <summary>
/// Provides resources stored in Google Drive using <see cref="UnityGoogleDrive"/> SDK.
/// </summary>
public class GoogleDriveResourceProvider : IResourceProvider
{
    #pragma warning disable 67
    public event Action<float> OnLoadProgress;
    #pragma warning restore 67

    public bool IsLoading { get { throw new System.NotImplementedException(); } }
    public float LoadProgress { get { throw new System.NotImplementedException(); } }

    public void LoadResourceAsync<T> (string path, Action<string, T> onLoaded = null) where T : UnityEngine.Object
    {
        throw new System.NotImplementedException();
    }

    public void UnloadResourceAsync (string path)
    {
        throw new System.NotImplementedException();
    }

    public T GetResource<T> (string path) where T : UnityEngine.Object
    {
        throw new System.NotImplementedException();
    }
}
