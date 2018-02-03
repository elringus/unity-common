using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestResourceProvider : MonoBehaviour
{
    public SpriteRenderer SpriteRenderer;
    public AudioSource AudioSource;

    private IResourceProvider provider;
    private string text = "empty";

    private readonly List<string> RESOURCES = new List<string>() {
        "Sprites/Image01",
        "Sprites/Image02",
        "Sprites/Image03",
        "Sprites/Image04",
    };

    private void Awake ()
    {
        //InitializeProjectResourceProvider();
        InitializeGoogleDriveResourceProvider();
    }

    private IEnumerator Start ()
    {
        //yield return ResolveByFullPath();
        //yield return ResolveTextByPath();
        //yield return ResolveSpritesByPath();
        //yield return ResolveFolders();
        //yield return TestResourcExists();
        yield return TestAudio();
    }

    private void OnGUI ()
    {
        GUILayout.TextArea(text);

        if (provider != null && provider.IsLoading)
            GUILayout.Label(provider.LoadProgress.ToString());
    }

    [ContextMenu("Test In Editor")]
    private void TestEditor ()
    {
        var provider = InitializeGoogleDriveResourceProvider();
        provider.LoadResources<string>("Text").Then(result => {
            foreach (var textResource in result) Debug.Log(textResource.Object);
        });
    }

    private static ProjectResourceProvider InitializeProjectResourceProvider ()
    {
        ProjectResourceProvider provider;

        if (Application.isPlaying) provider = Context.Resolve<ProjectResourceProvider>();
        else
        {
            var go = new GameObject();
            go.hideFlags = HideFlags.DontSave;
            provider = go.AddComponent<ProjectResourceProvider>();
        }

        provider.AddRedirector(new TextAssetToStringConverter());

        return provider;
    }

    private static GoogleDriveResourceProvider InitializeGoogleDriveResourceProvider ()
    {
        GoogleDriveResourceProvider provider;

        if (Application.isPlaying) provider = Context.Resolve<GoogleDriveResourceProvider>();
        else
        {
            var go = new GameObject();
            go.hideFlags = HideFlags.DontSave;
            provider = go.AddComponent<GoogleDriveResourceProvider>();
        }

        provider.DriveRootPath = "Resources";
        provider.ConcurrentRequestsLimit = 2;
        provider.AddConverter(new JpgOrPngToSpriteConverter());
        provider.AddConverter(new GDocToStringConverter());
        provider.AddConverter(new GFolderToFolderConverter());
        provider.AddConverter(new WavToAudioClipConverter());

        return provider;
    }

    private IEnumerator ResolveFolders ()
    {
        provider = Context.Resolve<IResourceProvider>();
        var loadAllAction = provider.LoadResources<Folder>(null);

        yield return loadAllAction;

        text = "completed";

        foreach (var folderResource in loadAllAction.Result)
        {
            text = folderResource.Object.Name;
            yield return new WaitForSeconds(1);
        }

        foreach (var textResource in loadAllAction.Result)
            provider.UnloadResource(textResource.Path);

        yield return new WaitForSeconds(3);
    }

    private IEnumerator ResolveSpritesByPath ()
    {
        provider = Context.Resolve<IResourceProvider>();
        var loadAllAction = provider.LoadResources<Sprite>("Sprites");

        yield return loadAllAction;

        foreach (var spriteResource in loadAllAction.Result)
        {
            SpriteRenderer.sprite = spriteResource.Object;
            yield return new WaitForSeconds(.5f);
        }

        foreach (var spriteResource in loadAllAction.Result)
            provider.UnloadResource(spriteResource.Path);
    }

    private IEnumerator TestAudio ()
    {
        provider = Context.Resolve<IResourceProvider>();
        var loadAllAction = provider.LoadResources<AudioClip>("Audio");

        yield return loadAllAction;

        foreach (var audioResource in loadAllAction.Result)
        {
            AudioSource.PlayOneShot(audioResource.Object);
            yield return new WaitForSeconds(audioResource.Object.length);
        }

        foreach (var audioResource in loadAllAction.Result)
            provider.UnloadResource(audioResource.Path);
    }

    private IEnumerator ResolveTextByPath ()
    {
        provider = Context.Resolve<IResourceProvider>();
        var loadAllAction = provider.LoadResources<string>("Text");

        yield return loadAllAction;

        foreach (var textResource in loadAllAction.Result)
        {
            text = textResource.Object;
            yield return new WaitForSeconds(1);
        }

        foreach (var textResource in loadAllAction.Result)
            provider.UnloadResource(textResource.Path);

        yield return new WaitForSeconds(3);
    }

    private IEnumerator TestResourcExists ()
    {
        provider = Context.Resolve<IResourceProvider>();

        foreach (var res in RESOURCES)
            yield return provider.ResourceExists<Sprite>(res).Then(b => print(res + ": " + b.ToString()));

    }

    private IEnumerator ResolveByFullPath ()
    {
        provider = Context.Resolve<IResourceProvider>();
        var waitFordelay = new WaitForSeconds(1.5f);

        foreach (var res in RESOURCES)
            provider.LoadResource<Sprite>(res);

        while (provider.IsLoading) yield return null;

        foreach (var res in RESOURCES)
        {
            SpriteRenderer.sprite = provider.LoadResource<Sprite>(res).Result.Object;
            yield return new WaitForSeconds(.5f);
        }

        yield return waitFordelay;

        foreach (var res in RESOURCES)
            provider.UnloadResource(res);

        yield return waitFordelay;

        foreach (var res in RESOURCES)
            provider.LoadResource<Sprite>(res);

        while (provider.IsLoading) yield return null;

        foreach (var res in RESOURCES)
        {
            SpriteRenderer.sprite = provider.LoadResource<Sprite>(res).Result.Object;
            yield return new WaitForSeconds(.5f);
        }
    }
}
