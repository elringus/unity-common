using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace UnityCommon
{
    /// <summary>
    /// Automatically saves active scene and all the assets on entering play mode.
    /// </summary>
    [InitializeOnLoad]
    public class AutoSave
    {
        static AutoSave ()
        {
            #if UNITY_2017_3_OR_NEWER
            EditorApplication.playModeStateChanged += _ => {
            #else
            EditorApplication.playmodeStateChanged = () => {
            #endif
                if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
                {
                    var activeScene = SceneManager.GetActiveScene();
                    EditorSceneManager.SaveScene(activeScene);
                    AssetDatabase.SaveAssets();
                }
            };
        }
    }
}
