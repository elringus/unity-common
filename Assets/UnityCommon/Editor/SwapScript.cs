using UnityEditor;
using UnityEngine;

namespace UnityCommon
{
    public class SwapScript : EditorWindow
    {
        private static readonly GUIContent searchScriptLabel = new GUIContent("Search Script", "The component to replace.");
        private static readonly GUIContent replacementScriptLabel = new GUIContent("Replacement Script", "The component to use as the replacement.");
        private static readonly GUIContent pathLabel = new GUIContent("Path", "Either folder where the prefabs are stored or a single prefab.");

        private MonoScript searchScript, replacementScript;
        private Object path;

        [MenuItem("Tools/Swap Script")]
        public static void OpenWindow ()
        {
            var position = new Rect(100, 100, 500, 160);
            GetWindowWithRect<SwapScript>(position, true, "Swap Script", true);
        }

        private void OnGUI ()
        {
            EditorGUILayout.LabelField("Swap Script", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("The tool allows to replace components of specified type attached to game objects of specified prefab or all the prefabs, stored at the specified folder (recursively).", EditorStyles.miniLabel);
            EditorGUILayout.Space();

            searchScript = (MonoScript)EditorGUILayout.ObjectField(searchScriptLabel, searchScript, typeof(MonoScript), false);
            replacementScript = (MonoScript)EditorGUILayout.ObjectField(replacementScriptLabel, replacementScript, typeof(MonoScript), false);
            path = EditorGUILayout.ObjectField(pathLabel, path, typeof(Object), false);

            var pathValid = ValidatePath();
            EditorGUI.BeginDisabledGroup(!searchScript || !replacementScript || !pathValid);
            if (GUILayout.Button("Perform Swap"))
            {
                try { PerformSwap(); }
                finally { EditorUtility.ClearProgressBar(); }
            }
            EditorGUI.EndDisabledGroup();
            if (path && !pathValid) EditorGUILayout.HelpBox("You've assigned an incorrect object to the `Path` field. Either a prefab or a folder is expected.", MessageType.Error);
        }

        private bool ValidatePath ()
        {
            if (!path) return false;
            if (path is GameObject) return true;
            var folderPath = AssetDatabase.GetAssetPath(path);
            return AssetDatabase.IsValidFolder(folderPath);
        }

        private void PerformSwap ()
        {
            if (path is GameObject singlePrefab)
            {
                ProcessPrefab(singlePrefab);
                return;
            }

            var folderPath = AssetDatabase.GetAssetPath(path);
            EditorUtility.DisplayProgressBar("Swapping Scripts", $"Loading assets at `{folderPath}`...", 0f);
            var assets = AssetDatabase.LoadAllAssetsAtPath(folderPath);
            for (int i = 0; i < assets.Length; i++)
            {
                var asset = assets[i];
                if (!(asset is GameObject prefab)) continue;
                var assetPath = AssetDatabase.GetAssetPath(path);
                EditorUtility.DisplayProgressBar("Swapping Scripts", $"Processing `{assetPath}`...", i / (float)assets.Length);
                ProcessPrefab(prefab);
            }

            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }

        private void ProcessPrefab (GameObject prefab)
        {
            prefab.ForEachDescendant(ProcessGameObject);
            EditorUtility.SetDirty(prefab);
        }

        private void ProcessGameObject (GameObject gameObject)
        {
            var searchType = searchScript.GetClass();
            var components = gameObject.GetComponents(searchType);
            foreach (var component in components)
                if (component is MonoBehaviour behaviour)
                    ProcessBehaviour(behaviour);
        }

        private void ProcessBehaviour (MonoBehaviour behaviour)
        {
            var serializedBehaviour = new SerializedObject(behaviour);
            var scriptProperty = serializedBehaviour.FindProperty("m_Script");
            serializedBehaviour.Update();
            scriptProperty.objectReferenceValue = replacementScript;
            serializedBehaviour.ApplyModifiedProperties();

            var prefabPath = AssetDatabase.GetAssetPath(behaviour.transform.root.gameObject);
            Debug.Log($"Script Swap: Replaced script for `{behaviour.GetType().Name}` component attached to `{behaviour.gameObject.name}` game object of `{prefabPath}` prefab.");
        }
    }
}
