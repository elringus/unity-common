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
        loader.LoadScene(SceneToLoadPath, TransitionScenePath, true, true);
    }

    private void LoadResources ()
    {
        var provider = Context.Resolve<GoogleDriveResourceProvider>();

        provider.DriveRootPath = "Resources";
        provider.ConcurrentRequestsLimit = 2;
        provider.AddConverter(new JpgOrPngToSpriteConverter());
        provider.AddConverter(new GDocToStringConverter());
        provider.AddConverter(new GFolderToFolderConverter());
        provider.AddConverter(new WavToAudioClipConverter());

        provider.LoadResources<AudioClip>("Audio");
    }
}
