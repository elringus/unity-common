using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UnityCommon
{
    public class RemoteResourceProvider : IResourceProvider
    {
        #pragma warning disable 67
        public event Action<float> OnLoadProgress;
        public event Action<string> OnMessage;
        #pragma warning restore 67

        public bool IsLoading { get { throw new System.NotImplementedException(); } }
        public float LoadProgress { get { throw new System.NotImplementedException(); } }

        public Task<Resource<T>> LoadResourceAsync<T> (string path) where T : class
        {
            throw new System.NotImplementedException();
        }

        public Task<List<Resource<T>>> LoadResourcesAsync<T> (string path) where T : class
        {
            throw new System.NotImplementedException();
        }

        public Task<List<Resource<T>>> LocateResourcesAsync<T> (string path) where T : class
        {
            throw new NotImplementedException();
        }

        public Task<bool> ResourceExistsAsync<T> (string path) where T : class
        {
            throw new System.NotImplementedException();
        }

        public void UnloadResource (string path)
        {
            throw new System.NotImplementedException();
        }

        public void UnloadResources ()
        {
            throw new NotImplementedException();
        }

        public bool ResourceLoaded (string path)
        {
            throw new System.NotImplementedException();
        }
    }
}
