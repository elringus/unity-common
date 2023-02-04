#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;

namespace UnityCommon
{
    public static class WebGLExtensions
    {
        /// <summary>
        /// Calls FS.syncfs in native js.
        /// </summary>
        [DllImport("__Internal")]
        public static extern void SyncFs ();

        /// <summary>
        /// Invokes window.open() with the specified parameters.
        /// https://developer.mozilla.org/en-US/docs/Web/API/Window/open
        /// </summary>
        [DllImport("__Internal")]
        public static extern void OpenURL (string url, string target);
    }
}
#endif
