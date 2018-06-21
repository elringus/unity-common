using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class PackageExporter : EditorWindow
{
    protected static string PackageName { get { return PlayerPrefs.GetString(PREFS_PREFIX + "PackageName"); } set { PlayerPrefs.SetString(PREFS_PREFIX + "PackageName", value); } }
    protected static string Copyright { get { return PlayerPrefs.GetString(PREFS_PREFIX + "Copyright"); } set { PlayerPrefs.SetString(PREFS_PREFIX + "Copyright", value); } }
    protected static string AssetsPath { get { return "Assets/" + PackageName; } }
    protected static string OutputPath { get { return PlayerPrefs.GetString(PREFS_PREFIX + "OutputPath"); } set { PlayerPrefs.SetString(PREFS_PREFIX + "OutputPath", value); } }
    protected static string OutputFileName { get { return PackageName; } }
    protected static string IgnoredPaths { get { return PlayerPrefs.GetString(PREFS_PREFIX + "IgnoredPaths"); } set { PlayerPrefs.SetString(PREFS_PREFIX + "IgnoredPaths", value); } }
    private static bool IsAnyPathsIgnored { get { return !string.IsNullOrEmpty(IgnoredPaths); } }
    protected static bool IsReadyToExport { get { return !string.IsNullOrEmpty(OutputPath) && !string.IsNullOrEmpty(OutputFileName); } }

    private const string TEMP_FOLDER_PATH = ".TEMP_PACKAGE_EXPORTER";
    private const string PREFS_PREFIX = "PackageExporter.";

    private void Awake ()
    {
        if (string.IsNullOrEmpty(PackageName))
            PackageName = Application.productName;
    }

    [MenuItem("Edit/Project Settings/Package Exporter")]
    private static void OpenSettingsWindow ()
    {
        var window = GetWindow<PackageExporter>();
        window.Show();
    }

    [MenuItem("Assets/+ Export Package", priority = 20)]
    private static void ExportPackage ()
    {
        if (IsReadyToExport)
            ExportPackageImpl();
    }

    [MenuItem("Assets/+ Export Assembly", priority = 20)]
    private static void ExportAssembly ()
    {
        if (IsReadyToExport)
            ExportAssemblyImpl();
    }

    private void OnGUI ()
    {
        EditorGUILayout.LabelField("Package Exporter Settings", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Settings are stored in editor's PlayerPrefs and won't be exposed in builds or project assets.", MessageType.Info);
        EditorGUILayout.Space();
        PackageName = EditorGUILayout.TextField("Package Name", PackageName);
        Copyright = EditorGUILayout.TextField("Copyright Notice", Copyright);
        using (new EditorGUILayout.HorizontalScope())
        {
            OutputPath = EditorGUILayout.TextField("Output Path", OutputPath);
            if (GUILayout.Button("Select", EditorStyles.miniButton, GUILayout.Width(65)))
                OutputPath = EditorUtility.OpenFolderPanel("Output Path", "", "");
        }
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Ignored paths (split with new line, start with 'Assets/...'): ");
        IgnoredPaths = EditorGUILayout.TextArea(IgnoredPaths);
    }

    private static void ExportAssemblyImpl ()
    {
        // Find all .asmdef files.
        var asset = AssetDatabase.LoadAssetAtPath<UnityEditorInternal.AssemblyDefinitionAsset>("");
        var assymblyDataType = Type.GetType("UnityEditor.Scripting.ScriptCompilation.CustomScriptAssemblyData");
        var assemblyData = JsonUtility.FromJson(asset.text, assymblyDataType);
        var assemblyName = assymblyDataType.GetField("name").GetValue(assemblyData);
        Debug.Log(assemblyName);
    }

    private static void ExportPackageImpl ()
    {
        // Copy package assets to temp folder and modify scripts and shaders (add copyright).
        DisplayProgressBar("Warming up...", 0f);
        var tmpFolderGuid = AssetDatabase.CreateFolder("Assets", TEMP_FOLDER_PATH);
        var tmpFolderPath = AssetDatabase.GUIDToAssetPath(tmpFolderGuid);
        var ignoredPaths = IsAnyPathsIgnored ? IgnoredPaths.SplitByNewLine().ToList() : new List<string>();
        var allAssetPaths = AssetDatabase.GetAllAssetPaths();
        for (int i = 0; i < allAssetPaths.Length; i++)
        {
            DisplayProgressBar("Processings assets...", ((float)i / allAssetPaths.Length) / 2f);
            var path = allAssetPaths[i];
            if (!path.StartsWith(AssetsPath)) continue;
            if (ignoredPaths.Exists(p => path.StartsWith(p))) continue;

            var copyPath = path.Replace(AssetsPath, tmpFolderPath);
            var copyDirectory = copyPath.GetBeforeLast("/");
            if (!Directory.Exists(copyDirectory))
            {
                Directory.CreateDirectory(copyDirectory);
                AssetDatabase.Refresh();
            }
            AssetDatabase.CopyAsset(path, copyPath);

            if (!copyPath.EndsWith(".cs") && !copyPath.EndsWith(".shader")) continue;
            var fullpath = Application.dataPath.Replace("Assets", "") + copyPath;
            var originalScriptText = File.ReadAllText(fullpath, Encoding.UTF8);
            string scriptText = string.Empty;
            var isImportedScript = copyPath.Contains("ThirdParty");
            var copyright = isImportedScript || string.IsNullOrEmpty(Copyright) ? string.Empty : "// " + Copyright;
            if (!string.IsNullOrEmpty(copyright) && !isImportedScript)
                scriptText += copyright + Environment.NewLine + Environment.NewLine + originalScriptText;
            File.WriteAllText(fullpath, scriptText, Encoding.UTF8);
        }

        // Export the package.
        DisplayProgressBar("Writing package file...", .5f);
        AssetDatabase.ExportPackage(tmpFolderPath, OutputPath + "/" + OutputFileName + ".unitypackage", ExportPackageOptions.Recurse);

        // Delete temp folder.
        AssetDatabase.DeleteAsset(tmpFolderPath);

        EditorUtility.ClearProgressBar();
    }

    private static void DisplayProgressBar (string activity, float progress)
    {
        EditorUtility.DisplayProgressBar(string.Format("Exporting {0}", PackageName), activity, progress);
    }
}
