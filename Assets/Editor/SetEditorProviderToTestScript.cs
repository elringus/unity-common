using UnityCommon;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class SetEditorProviderToTestScript
{
    static SetEditorProviderToTestScript ()
    {
        var provider = new EditorResourceProvider();
        var editorResources = Object.FindObjectOfType<TestResourceProvider>().EditorResources;

        foreach (var resource in editorResources)
        {
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(resource.Object, out string guid, out long id);
            provider.AddResourceGuid(resource.Path, guid);
        }

        TestResourceProvider.EditorProvider = provider;
    }
}
