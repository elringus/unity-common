using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class PackageExporter : EditorWindow
{
    const string TEMP_PATH = "Assets/TEMP/";

    private bool IsReadyToExport { get { return !string.IsNullOrEmpty(outputPath) && 
                                                !string.IsNullOrEmpty(outputFileName); } }

    private string outputPath = null;
    private string outputFileName = null;
    private string namespaceToWrap;
    private Dictionary<string, string> modifiedScripts;

    [MenuItem("Assets/Export Package (Advanced)...", priority = 20)]
    public static void Open ()
    {
        GetWindow<PackageExporter>();
    }

    private void OnEnable ()
    {
        InitializeEditor();
    }

    public void OnGUI ()
    {
        DrawUI();
    }

    private void InitializeEditor ()
    {
        modifiedScripts = new Dictionary<string, string>();
        titleContent = new GUIContent("Package Exporter");
        minSize = new Vector2(600f, 300f);
    }

    private void DrawUI ()
    {
        if (!string.IsNullOrEmpty(outputPath))
            GUILayout.Label(string.Format("Output path: {0}", outputPath));

        outputFileName = EditorGUILayout.TextField("Output file name: ", outputFileName);
        namespaceToWrap = EditorGUILayout.TextField("Namespace: ", namespaceToWrap);

        if (GUILayout.Button("Select Output Path"))
            SelectOutputPath();

        if (IsReadyToExport && GUILayout.Button("Select Assets And Export"))
            SelectAssetsAndExport();
    }

    private void SelectOutputPath ()
    {
        outputPath = EditorUtility.OpenFolderPanel("Load png Textures", "", "");
    }

    private void SelectAssetsAndExport ()
    {
        if (string.IsNullOrEmpty(outputPath))
        {
            Debug.LogError("Select output path and file name.");
            return;
        }

        var assetsPath = EditorUtility.OpenFolderPanel("Select Assets", "", "");
        assetsPath = assetsPath.Replace(assetsPath.GetBefore("Assets"), "");

        if (!string.IsNullOrEmpty(namespaceToWrap))
        {
            foreach (var path in AssetDatabase.GetAllAssetPaths())
            {
                if (!path.StartsWith(assetsPath)) continue;
                if (!path.EndsWith(".cs")) continue;

                var fullpath = Application.dataPath.Replace("Assets", "") + path;

                var scriptText = "namespace " + namespaceToWrap + Environment.NewLine + "{" + Environment.NewLine + Environment.NewLine;
                var originalScriptText = File.ReadAllText(fullpath, Encoding.UTF8);
                scriptText += originalScriptText + Environment.NewLine + "}" + Environment.NewLine;
                File.WriteAllText(fullpath, scriptText, Encoding.UTF8);

                modifiedScripts.Add(fullpath, originalScriptText);
            }
        }

        AssetDatabase.ExportPackage(assetsPath, outputPath + "/" + outputFileName + ".unitypackage", ExportPackageOptions.Recurse);

        if (!string.IsNullOrEmpty(namespaceToWrap))
        {
            foreach (var modifiedScript in modifiedScripts)
            {
                File.WriteAllText(modifiedScript.Key, modifiedScript.Value, Encoding.UTF8);
            }
        }
    }
}
