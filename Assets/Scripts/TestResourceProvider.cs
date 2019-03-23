using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;

public class TestResourceProvider : MonoBehaviour
{
    [Serializable]
    public class PathToObj { public string Path; public UnityEngine.Object Object; }

    public SpriteRenderer SpriteRenderer;
    public AudioSource AudioSource;
    public PathToObj[] EditorResources;

    private IResourceProvider provider;
    private string text = "empty";

    private readonly List<string> resources = new List<string>() {
        "Sprites/Image01",
        "Sprites/Image02",
        "Sprites/Image03",
    };

    private void Awake ()
    {
        provider = InitializeProjectResourceProvider();
        //provider = InitializeEditorResourceProvider();
        //provider = InitializeGoogleDriveResourceProvider(false);
        //provider = InitializeLocalResourceProvider();
    }

    private async void Start ()
    {
        await new WaitForEndOfFrame();

        await ResolveByFullPathAsync();
        await ResolveTextByPathAsync();
        await ResolveFoldersAsync();
        await TestResourceExistsAsync();
        await TestAudioAsync();
        await TestUnloadAsync();
        await TestTextureResources();
        await TestTextureByDir();
        //await TestNullPropagation();
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
        provider = InitializeGoogleDriveResourceProvider(false);
        TestEditorAsync().WrapAsync();
    }

    private async Task TestEditorAsync ()
    {
        var result = (await provider.LoadResourcesAsync<TextAsset>("Text")).ToList();
        for (int i = 0; i < result.Count; i++)
            Debug.Log($"{i}: {result[i].Object.text}");
    }

    private EditorResourceProvider InitializeEditorResourceProvider ()
    {
        var provider = new EditorResourceProvider();

        #if UNITY_EDITOR
        foreach (var resource in EditorResources)
        {
            UnityEditor.AssetDatabase.TryGetGUIDAndLocalFileIdentifier(resource.Object, out string guid, out long id);
            provider.AddResourceGuid(resource.Path, guid);
        }
        #endif

        return provider;
    }

    private static ProjectResourceProvider InitializeProjectResourceProvider ()
    {
        var provider = new ProjectResourceProvider();

        return provider;
    }

    private IResourceProvider InitializeGoogleDriveResourceProvider (bool purgeCache)
    {
        #if UNITY_GOOGLE_DRIVE_AVAILABLE
        var provider = new GoogleDriveResourceProvider("Resources", GoogleDriveResourceProvider.CachingPolicyType.Smart, 2);

        provider.AddConverter(new JpgOrPngToSpriteConverter());
        provider.AddConverter(new JpgOrPngToTextureConverter());
        provider.AddConverter(new GDocToTextAssetConverter());
        //provider.AddConverter(new WavToAudioClipConverter());
        provider.AddConverter(new Mp3ToAudioClipConverter());

        if (purgeCache) provider.PurgeCache();

        return provider;
        #else
        return null;
        #endif
    }

    private static LocalResourceProvider InitializeLocalResourceProvider ()
    {
        var provider = new LocalResourceProvider("Resources");

        provider.AddConverter(new JpgOrPngToSpriteConverter());
        provider.AddConverter(new JpgOrPngToTextureConverter());
        provider.AddConverter(new TxtToTextAssetConverter());
        provider.AddConverter(new WavToAudioClipConverter());
        //provider.AddConverter(new Mp3ToAudioClipConverter());

        return provider;
    }

    private async Task ResolveFoldersAsync ()
    {
        text = "Starting resolving folders...";
        await Task.Delay(TimeSpan.FromSeconds(1f));

        var folders = await provider.LocateFoldersAsync(null);

        text = $"Finished resolving folders. Found {folders.Count()} folders.";
        await Task.Delay(TimeSpan.FromSeconds(1f));

        foreach (var folder in folders)
        {
            text = folder.Name;
            await Task.Delay(TimeSpan.FromSeconds(1f));
        }
    }

    private async Task TestUnloadAsync ()
    {
        for (int i = 0; i < 10; i++)
        {
            var resources = await provider.LoadResourcesAsync<Texture2D>("Sprites");
            text = "Total memory used after load: " + Mathf.CeilToInt(System.GC.GetTotalMemory(true) * .000001f) + "Mb";

            await Task.Delay(TimeSpan.FromSeconds(.5f));

            foreach (var resource in resources)
                await provider.UnloadResourceAsync(resource.Path);
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
            await Task.Delay(TimeSpan.FromSeconds(5));
            AudioSource.Stop();
        }

        foreach (var audioResource in resources)
            await provider.UnloadResourceAsync(audioResource.Path);
    }

    private async Task ResolveTextByPathAsync ()
    {
        var resources = await provider.LoadResourcesAsync<TextAsset>("Text");

        foreach (var textResource in resources)
        {
            text = textResource.Object.text;
            await Task.Delay(TimeSpan.FromSeconds(1f));
        }

        foreach (var textResource in resources)
            await provider.UnloadResourceAsync(textResource.Path);
    }

    private async Task TestResourceExistsAsync ()
    {
        foreach (var res in resources)
        {
            var exist = await provider.ResourceExistsAsync<Sprite>(res);
            print(res + ": " + exist.ToString());
        }
    }

    private async Task ResolveByFullPathAsync ()
    {
        foreach (var res in resources)
            await provider.LoadResourceAsync<Sprite>(res);

        foreach (var res in resources)
        {
            SpriteRenderer.sprite = (await provider.LoadResourceAsync<Sprite>(res)).Object;
            await Task.Delay(TimeSpan.FromSeconds(.5f));
        }

        await Task.Delay(TimeSpan.FromSeconds(1.5f));

        foreach (var res in resources)
            await provider.UnloadResourceAsync(res);

        await Task.Delay(TimeSpan.FromSeconds(1.5f));

        foreach (var res in resources)
            await provider.LoadResourceAsync<Sprite>(res);

        foreach (var res in resources)
        {
            SpriteRenderer.sprite = (await provider.LoadResourceAsync<Sprite>(res)).Object;
            await Task.Delay(TimeSpan.FromSeconds(.5f));
        }
    }

    private async Task TestTextureResources ()
    {
        foreach (var res in resources)
            await provider.LoadResourceAsync<Texture2D>(res);

        foreach (var res in resources)
        {
            var texture = (await provider.LoadResourceAsync<Texture2D>(res)).Object;
            SpriteRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * .5f);
            await Task.Delay(TimeSpan.FromSeconds(.5f));
        }
    }

    private async Task TestTextureByDir ()
    {
        var resources = await provider.LoadResourcesAsync<Texture2D>("Sprites");

        foreach (var res in resources)
        {
            var texture = res.Object;
            SpriteRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * .5f);
            await Task.Delay(TimeSpan.FromSeconds(.5f));
        }
    }

    private async Task TestNullPropagation ()
    {
        var loader = new ResourceLoader<Texture2D>(new List<IResourceProvider> { provider }, "Sprites");
        var image = await loader.LoadAsync("Image09");
        print(image);
        print("Propagated!");
    }
}
