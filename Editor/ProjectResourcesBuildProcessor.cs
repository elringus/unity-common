using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace UnityCommon
{
    public class ProjectResourcesBuildProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public static string TempFolderPath = "Assets/TEMP_UNITY_COMMON/Resources";

        public int callbackOrder => 100;

        private static string assetPath => $"{TempFolderPath}/{ProjectResources.ResourcePath}.asset";

        public void OnPreprocessBuild (BuildReport report)
        {
            var asset = ProjectResources.Get();
            EditorUtils.CreateFolderAsset(assetPath.GetBeforeLast("/"));
            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();
        }

        public void OnPostprocessBuild (BuildReport report)
        {
            AssetDatabase.DeleteAsset(TempFolderPath.GetBeforeLast("/"));
            AssetDatabase.SaveAssets();
        }
    }
}
