using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityCommon
{
    public class ProjectResources : ScriptableObject
    {
        public List<string> ResourcePaths => resourcePaths;

        [SerializeField] List<string> resourcePaths = new List<string>();

        private void Awake ()
        {
            LocateAllResources();
        }

        public static ProjectResources Get ()
        {
            return Application.isEditor ? CreateInstance<ProjectResources>() : Resources.Load<ProjectResources>(nameof(ProjectResources));
        }

        public void LocateAllResources ()
        {
            #if UNITY_EDITOR
            resourcePaths.Clear();
            WalkDirectoryTree(new System.IO.DirectoryInfo(Application.dataPath), resourcePaths, false);
            #endif
        }

        #if UNITY_EDITOR
        private static void WalkDirectoryTree (System.IO.DirectoryInfo directory, List<string> outPaths, bool isInsideResources)
        {
            var subDirs = directory.GetDirectories();
            foreach (var dirInfo in subDirs)
            {
                if (!isInsideResources && dirInfo.Name != "Resources") continue;
                if (!isInsideResources && dirInfo.Name == "Resources") WalkDirectoryTree(dirInfo, outPaths, true);

                if (isInsideResources)
                {
                    var paths = dirInfo.GetFiles().Where(p => !p.FullName.EndsWithFast(".meta"))
                        .Select(p => p.FullName.Replace("\\", "/").GetAfterFirst("/Resources/").GetBeforeLast("."));
                    outPaths.AddRange(paths);

                    WalkDirectoryTree(dirInfo, outPaths, true);
                }
            }
        }
        #endif
    }

    #if UNITY_EDITOR
    public class ProjectResourcesPreprocessor : UnityEditor.Build.IPreprocessBuildWithReport, UnityEditor.Build.IPostprocessBuildWithReport
    {
        public int callbackOrder => 100;

        protected string DirectoryPath => "Assets/TEMP_UNITY_COMMON/Resources"; 
        protected string AssetPath => DirectoryPath + $"/{nameof(ProjectResources)}.asset";

        private bool folderCreated;

        public void OnPreprocessBuild (UnityEditor.Build.Reporting.BuildReport report)
        {
            var asset = ScriptableObject.CreateInstance<ProjectResources>();
            asset.LocateAllResources();

            if (!System.IO.Directory.Exists(DirectoryPath))
            {
                System.IO.Directory.CreateDirectory(DirectoryPath);
                folderCreated = true;
            }
            else folderCreated = false;

            UnityEditor.AssetDatabase.CreateAsset(asset, AssetPath);
            UnityEditor.AssetDatabase.SaveAssets();
        }

        public void OnPostprocessBuild (UnityEditor.Build.Reporting.BuildReport report)
        {
            UnityEditor.AssetDatabase.DeleteAsset(AssetPath);
            if (folderCreated) UnityEditor.AssetDatabase.DeleteAsset(DirectoryPath);
            UnityEditor.AssetDatabase.SaveAssets();
        }
    }
    #endif
}
