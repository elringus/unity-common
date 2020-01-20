using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace UnityCommon
{
    public class ProjectResourcesBuildProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public static string TempFolderPath = "Assets/TEMP_UNITY_COMMON/Resources";

        public int callbackOrder => 100;

        private static string assetPath => TempFolderPath + "/" + nameof(ProjectResources) + ".asset";

        public void OnPreprocessBuild (UnityEditor.Build.Reporting.BuildReport report)
        {
            var asset = ScriptableObject.CreateInstance<ProjectResources>();
            asset.LocateAllResources();
            EditorUtils.CreateFolderAsset(TempFolderPath);
            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();
        }

        public void OnPostprocessBuild (UnityEditor.Build.Reporting.BuildReport report)
        {
            AssetDatabase.DeleteAsset(TempFolderPath.GetBeforeLast("/"));
            AssetDatabase.SaveAssets();
        }
    }
}
