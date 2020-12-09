using UnityEngine;

namespace UnityCommon
{
    public class ApplicationBehaviour : MonoBehaviour
    {
        public static ApplicationBehaviour Instance => ObjectUtils.IsValid(instanceCache) ? instanceCache : CreateSingleton();

        private static ApplicationBehaviour instanceCache;

        private static ApplicationBehaviour CreateSingleton ()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("ApplicationBehaviour doesn't work at edit time.");
                return null;
            }

            instanceCache = new GameObject("ApplicationBehaviour").AddComponent<ApplicationBehaviour>();
            instanceCache.gameObject.hideFlags = HideFlags.DontSave;
            DontDestroyOnLoad(instanceCache.gameObject);
            return instanceCache;
        }

        private void OnDestroy ()
        {
            instanceCache = null;
        }
    }
}
