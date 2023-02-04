using UnityEngine;

namespace UnityCommon
{
    public static class WebUtils
    {
        public static AudioType EvaluateAudioTypeFromMime (string mimeType)
        {
            switch (mimeType)
            {
                case "audio/aiff": return AudioType.AIFF;
                case "audio/mpeg": return AudioType.MPEG;
                case "audio/mpeg3": return AudioType.MPEG;
                case "audio/mp3": return AudioType.MPEG;
                case "audio/ogg": return AudioType.OGGVORBIS;
                case "video/ogg": return AudioType.OGGVORBIS;
                case "audio/wav": return AudioType.WAV;
                default: return AudioType.UNKNOWN;
            }
        }

        /// <summary>
        /// Navigates to the specified URL using default or current web browser.
        /// </summary>
        /// <remarks>
        /// When used outside of WebGL or in editor will use <see cref="Application.OpenURL"/>,
        /// otherwise native window.open() JS function is used.
        /// </remarks>
        /// <param name="url">The URL to navigate to.</param>
        /// <param name="target">Browsing context: _self, _blank, _parent, _top. Not supported outside of WebGL.</param>
        public static void OpenURL (string url, string target = "_self")
        {
            #if UNITY_WEBGL && !UNITY_EDITOR
            WebGLExtensions.OpenURL(url, target);
            #else
            Application.OpenURL(url);
            #endif
        }
    }
}
