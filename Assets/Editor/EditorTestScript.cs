using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EditorTestScript
{
    [MenuItem("Test/Install Shader")]
    private static void InstallShader ()
    {
        BlendModes.ExtensionsManager.InstallShaderExtension("UIDefault");
    }

    [MenuItem("Test/Install Comp")]
    private static void InstallComp ()
    {
        BlendModes.ExtensionsManager.InstallComponentExtension("UnityEngine.UI.Image");
    }

    [MenuItem("Test/Remove Shader")]
    private static void RemoveShader ()
    {
        BlendModes.ExtensionsManager.RemoveShaderExtension("UIDefault");
    }

    [MenuItem("Test/Remove Comp")]
    private static void RemoveComp ()
    {
        BlendModes.ExtensionsManager.RemoveComponentExtension("UnityEngine.UI.Image");
    }
}
