using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    };

    private void Awake ()
    {
        //provider = InitializeProjectResourceProvider();
        provider = InitializeGoogleDriveResourceProvider();
        //provider = InitializeLocalResourceProvider();
    }

    private async void Start ()
    {
        //await ResolveByFullPathAsync();
        //await ResolveTextByPathAsync();
        //await ResolveFoldersAsync();
        //await TestResourceExistsAsync();
        //await TestAudioAsync();
        //await TestUnloadAsync();
        await TestTextureResources();
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
        provider = InitializeGoogleDriveResourceProvider();
        TestEditorAsync().WrapAsync();
    }

    private async Task TestEditorAsync ()
    {
        var result = await provider.LoadResourcesAsync<string>("Text");
        for (int i = 0; i < result.Count; i++)
            Debug.Log($"{i}: {result[i].Object}");
    }

    private static ProjectResourceProvider InitializeProjectResourceProvider ()
    {
        ProjectResourceProvider provider;
        var go = new GameObject();
        go.hideFlags = HideFlags.DontSave;
        provider = go.AddComponent<ProjectResourceProvider>();

        provider.AddRedirector(new TextAssetToStringConverter());

        return provider;
    }

    private GoogleDriveResourceProvider InitializeGoogleDriveResourceProvider ()
    {
        GoogleDriveResourceProvider provider;
        var go = new GameObject();
        go.hideFlags = HideFlags.DontSave;
        provider = go.AddComponent<GoogleDriveResourceProvider>();

        provider.DriveRootPath = "Resources";
        provider.ConcurrentRequestsLimit = 2;
        provider.CachingPolicy = GoogleDriveResourceProvider.CachingPolicyType.Smart;
        provider.AddConverter(new JpgOrPngToSpriteConverter());
        provider.AddConverter(new JpgOrPngToTextureConverter());
        provider.AddConverter(new GDocToStringConverter());
        provider.AddConverter(new GFolderToFolderConverter());
        //provider.AddConverter(new WavToAudioClipConverter());
        provider.AddConverter(new Mp3ToAudioClipConverter());

        return provider;
    }

    private static LocalResourceProvider InitializeLocalResourceProvider ()
    {
        LocalResourceProvider provider;
        var go = new GameObject();
        go.hideFlags = HideFlags.DontSave;
        provider = go.AddComponent<LocalResourceProvider>();

        provider.RootPath = "Resources";
        provider.AddConverter(new DirectoryToFolderConverter());
        provider.AddConverter(new JpgOrPngToSpriteConverter());
        provider.AddConverter(new JpgOrPngToTextureConverter());
        provider.AddConverter(new TxtToStringConverter());
        //provider.AddConverter(new WavToAudioClipConverter());
        provider.AddConverter(new Mp3ToAudioClipConverter());

        return provider;
    }

    private async Task ResolveFoldersAsync ()
    {
        var resources = await provider.LoadResourcesAsync<Folder>(null);

        text = "completed";

        foreach (var folderResource in resources)
        {
            text = folderResource.Object.Name;
            await Task.Delay(TimeSpan.FromSeconds(1f));
        }

        foreach (var textResource in resources)
            provider.UnloadResource(textResource.Path);
    }

    private async Task TestUnloadAsync ()
    {
        for (int i = 0; i < 10; i++)
        {
            var resources = await provider.LoadResourcesAsync<AudioClip>("Unload");
            text = "Total memory used after load: " + Mathf.CeilToInt(System.GC.GetTotalMemory(true) * .000001f) + "Mb";

            await Task.Delay(TimeSpan.FromSeconds(.5f));

            foreach (var resource in resources)
                provider.UnloadResource(resource.Path);
            text = "Total memory used after unload: " + Mathf.CeilToInt(System.GC.GetTotalMemory(true) * .000001f) + "Mb";

            await Task.Delay(TimeSpan.FromSeconds(.5f));
        }
    }

    private async Task TestAudioAsync ()
    {
        var resources = await provider.LoadResourcesAsync<AudioClip>("Audio");

        foreach (var audioResource in resources)
        {
            AudioSource.PlayOneShot(audioResource.Object);
            await Task.Delay(TimeSpan.FromSeconds(audioResource.Object.length));
        }

        foreach (var audioResource in resources)
            provider.UnloadResource(audioResource.Path);
    }

    private async Task ResolveTextByPathAsync ()
    {
        var resources = await provider.LoadResourcesAsync<string>("Text");

        foreach (var textResource in resources)
        {
            text = textResource.Object;
            await Task.Delay(TimeSpan.FromSeconds(1f));
        }

        foreach (var textResource in resources)
            provider.UnloadResource(textResource.Path);
    }

    private async Task TestResourceExistsAsync ()
    {
        foreach (var res in RESOURCES)
        {
            var exist = await provider.ResourceExistsAsync<Sprite>(res);
            print(res + ": " + exist.ToString());
        }
    }

    private async Task ResolveByFullPathAsync ()
    {
        foreach (var res in RESOURCES)
            await provider.LoadResourceAsync<Sprite>(res);

        foreach (var res in RESOURCES)
        {
            SpriteRenderer.sprite = (await provider.LoadResourceAsync<Sprite>(res)).Object;
            await Task.Delay(TimeSpan.FromSeconds(.5f));
        }

        await Task.Delay(TimeSpan.FromSeconds(1.5f));

        foreach (var res in RESOURCES)
            provider.UnloadResource(res);

        await Task.Delay(TimeSpan.FromSeconds(1.5f));

        foreach (var res in RESOURCES)
            await provider.LoadResourceAsync<Sprite>(res);

        foreach (var res in RESOURCES)
        {
            SpriteRenderer.sprite = (await provider.LoadResourceAsync<Sprite>(res)).Object;
            await Task.Delay(TimeSpan.FromSeconds(.5f));
        }
    }

    private async Task TestTextureResources ()
    {
        foreach (var res in RESOURCES)
            await provider.LoadResourceAsync<Texture2D>(res);

        foreach (var res in RESOURCES)
        {
            var texture = (await provider.LoadResourceAsync<Texture2D>(res)).Object;
            SpriteRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * .5f);
            await Task.Delay(TimeSpan.FromSeconds(.5f));
        }
    }
}
