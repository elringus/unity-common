using UnityCommon;
using UnityEditor;
using UnityEngine;

public class SetEditorProviderToTestScript
{
    [InitializeOnLoadMethod]
    private static void InitializeEditorProvider ()
    {
        // Also executes when entering play mode.
        var provider = new EditorResourceProvider();
        var testProviderObj = Object.FindObjectOfType<TestResourceProvider>();
        if (!ObjectUtils.IsValid(testProviderObj)) return;

        foreach (var resource in testProviderObj.EditorResources)
        {
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(resource.Object, out string guid, out long id);
            provider.AddResourceGuid(resource.Path, guid);
        }

        TestResourceProvider.EditorProvider = provider;
    }
}
