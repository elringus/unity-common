using UnityCommon;
using UnityEngine;

public class TestSceneLoader : MonoBehaviour
{
    public string SceneToLoadPath;
    public string TransitionScenePath;

    public void OnEnable ()
    {
        Context.Resolve<GoogleDriveResourceProvider>();

        var loader = Context.Resolve<SceneLoader>();
        loader.OnReadyToActivate += LoadResources;
        loader.LoadSceneAsync(SceneToLoadPath, TransitionScenePath, true, true).WrapAsync();
    }

    private void LoadResources ()
    {
        var provider = new GoogleDriveResourceProvider("Resources", GoogleDriveResourceProvider.CachingPolicyType.Smart, 2);

        provider.AddConverter(new JpgOrPngToSpriteConverter());
        provider.AddConverter(new GDocToStringConverter());
        provider.AddConverter(new GFolderToFolderConverter());
        provider.AddConverter(new WavToAudioClipConverter());

        provider.LoadResourcesAsync<AudioClip>("Audio").WrapAsync();
    }
}
