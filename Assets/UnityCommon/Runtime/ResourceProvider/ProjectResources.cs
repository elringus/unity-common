using System.Collections.Generic;
using UnityEngine;

namespace UnityCommon
{
    public class ProjectResources : ScriptableObject
    {
        [SerializeField] List<Folder> folders = new List<Folder>();

        public static ProjectResources Get ()
        {
            return Application.isEditor ? CreateInstance<ProjectResources>() : Resources.Load<ProjectResources>("ProjectResources");
        }

        public List<Folder> LocateAllResourceFolders ()
        {
            #if UNITY_EDITOR
            folders.Clear();
            WalkDirectoryTree(new System.IO.DirectoryInfo(Application.dataPath), ref folders, false);
            #endif
            return folders;
        }

        #if UNITY_EDITOR
        private static void WalkDirectoryTree (System.IO.DirectoryInfo directory, ref List<Folder> outFolders, bool isInsideResources)
        {
            var subDirs = directory.GetDirectories();
            foreach (var dirInfo in subDirs)
            {
                if (!isInsideResources && dirInfo.Name != "Resources") continue;
                if (!isInsideResources && dirInfo.Name == "Resources") WalkDirectoryTree(dirInfo, ref outFolders, true);

                if (isInsideResources)
                {
                    var folder = new Folder(dirInfo.FullName.Replace("\\", "/").GetAfterFirst("/Resources"));
                    outFolders.Add(folder);
                    WalkDirectoryTree(dirInfo, ref outFolders, true);
                }
            }
        }
        #endif
    }

    #if UNITY_EDITOR
    public class ProjectResourcesPreprocessor :
        #if UNITY_2018_1_OR_NEWER
        UnityEditor.Build.IPreprocessBuildWithReport, UnityEditor.Build.IPostprocessBuildWithReport
        #else
        UnityEditor.Build.IPreprocessBuild, UnityEditor.Build.IPostprocessBuild
        #endif
    {
        public int callbackOrder { get { return 0; } }

        protected string DIR_PATH { get { return "Assets/Resources"; } }
        protected string ASSET_PATH { get { return DIR_PATH + "/ProjectResources.asset"; } }

        private bool folderCreated;

        public void OnPreprocessBuild (UnityEditor.BuildTarget target, string path)
        {
            var asset = ScriptableObject.CreateInstance<ProjectResources>();
            asset.LocateAllResourceFolders();

            if (!System.IO.Directory.Exists(DIR_PATH))
            {
                System.IO.Directory.CreateDirectory(DIR_PATH);
                folderCreated = true;
            }
            else folderCreated = false;

            UnityEditor.AssetDatabase.CreateAsset(asset, ASSET_PATH);
            UnityEditor.AssetDatabase.SaveAssets();
        }

        public void OnPostprocessBuild (UnityEditor.BuildTarget target, string path)
        {
            UnityEditor.AssetDatabase.DeleteAsset(ASSET_PATH);
            if (folderCreated) UnityEditor.AssetDatabase.DeleteAsset(DIR_PATH);
            UnityEditor.AssetDatabase.SaveAssets();
        }

        #if UNITY_2018_1_OR_NEWER
        public void OnPreprocessBuild (UnityEditor.Build.Reporting.BuildReport report)
        {
            OnPreprocessBuild(report.summary.platform, report.summary.outputPath);
        }

        public void OnPostprocessBuild (UnityEditor.Build.Reporting.BuildReport report)
        {
            OnPostprocessBuild(report.summary.platform, report.summary.outputPath);
        }
        #endif
    }
    #endif
}
