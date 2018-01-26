using System.Collections.Generic;
using UnityEngine;

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
public class ProjectResourcesPreprocessor : UnityEditor.Build.IPreprocessBuild, UnityEditor.Build.IPostprocessBuild
{
    public int callbackOrder { get { return 0; } }

    private const string PATH = "Assets/Resources/ProjectResources.asset";

    public void OnPreprocessBuild (UnityEditor.BuildTarget target, string path)
    {
        var asset = ScriptableObject.CreateInstance<ProjectResources>();
        asset.LocateAllResourceFolders();
        UnityEditor.AssetDatabase.CreateAsset(asset, PATH);
        UnityEditor.AssetDatabase.SaveAssets();
    }

    public void OnPostprocessBuild (UnityEditor.BuildTarget target, string path)
    {
        UnityEditor.AssetDatabase.DeleteAsset(PATH);
        UnityEditor.AssetDatabase.SaveAssets();
    }
}
#endif
