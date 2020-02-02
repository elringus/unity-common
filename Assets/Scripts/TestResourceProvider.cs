using System;
using System.Collections.Generic;
using System.Linq;
using UnityCommon;
using UnityEngine;
using UniRx.Async;

public class TestResourceProvider : MonoBehaviour
{
    public static IResourceProvider EditorProvider;

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
        //provider = InitializeProjectResourceProvider();
        //provider = InitializeEditorResourceProvider();
        //provider = InitializeLocalResourceProvider();
        provider = InitializeGoogleDriveResourceProvider(true);
        //provider = InitializeAddresableResourceProvider();
    }

    private async void Start ()
    {
        await AsyncUtils.WaitEndOfFrame;

        await ResolveByFullPathAsync();
        await ResolveTextByPathAsync();
        await ResolveFoldersAsync();
        await TestResourceExistsAsync();
        await TestAudioAsync();
        await TestUnloadAsync();
        await TestTextureResources();
        await TestTextureByDir();
        await TestNullPropagation();
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
        TestEditorAsync().Forget();
    }

    private async UniTask TestEditorAsync ()
    {
        var result = (await provider.LoadResourcesAsync<TextAsset>("Text")).ToList();
        for (int i = 0; i < result.Count; i++)
            Debug.Log($"{i}: {result[i].Object.text}");
    }

    private IResourceProvider InitializeEditorResourceProvider ()
    {
        return EditorProvider;
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

    private static AddressableResourceProvider InitializeAddresableResourceProvider ()
    {
        var provider = new AddressableResourceProvider();
        return provider;
    }

    private async UniTask ResolveFoldersAsync ()
    {
        text = "Starting resolving folders...";
        await UniTask.Delay(TimeSpan.FromSeconds(1f));

        var folders = await provider.LocateFoldersAsync(null);

        text = $"Finished resolving folders. Found {folders.Count()} folders.";
        await UniTask.Delay(TimeSpan.FromSeconds(1f));

        foreach (var folder in folders)
        {
            text = folder.Name;
            await UniTask.Delay(TimeSpan.FromSeconds(1f));
        }
    }

    private async UniTask TestUnloadAsync ()
    {
        for (int i = 0; i < 10; i++)
        {
            var resources = await provider.LoadResourcesAsync<Texture2D>("Sprites");
            text = "Total memory used after load: " + Mathf.CeilToInt(System.GC.GetTotalMemory(true) * .000001f) + "Mb";

            await UniTask.Delay(TimeSpan.FromSeconds(.5f));

            foreach (var resource in resources)
                provider.UnloadResource(resource.Path);
            text = "Total memory used after unload: " + Mathf.CeilToInt(System.GC.GetTotalMemory(true) * .000001f) + "Mb";

            await UniTask.Delay(TimeSpan.FromSeconds(.5f));
        }
    }

    private async UniTask TestAudioAsync ()
    {
        var resources = await provider.LoadResourcesAsync<AudioClip>("Audio");

        foreach (var audioResource in resources)
        {
            AudioSource.PlayOneShot(audioResource.Object);
            await UniTask.Delay(TimeSpan.FromSeconds(5));
            AudioSource.Stop();
        }

        foreach (var audioResource in resources)
            provider.UnloadResource(audioResource.Path);
    }

    private async UniTask ResolveTextByPathAsync ()
    {
        var resources = await provider.LoadResourcesAsync<TextAsset>("Text");

        foreach (var textResource in resources)
        {
            text = textResource.Object.text;
            await UniTask.Delay(TimeSpan.FromSeconds(1f));
        }

        foreach (var textResource in resources)
            provider.UnloadResource(textResource.Path);
    }

    private async UniTask TestResourceExistsAsync ()
    {
        foreach (var res in resources)
        {
            var exist = await provider.ResourceExistsAsync<Sprite>(res);
            print(res + ": " + exist.ToString());
        }
    }

    private async UniTask ResolveByFullPathAsync ()
    {
        foreach (var res in resources)
            await provider.LoadResourceAsync<Sprite>(res);

        foreach (var res in resources)
        {
            SpriteRenderer.sprite = (await provider.LoadResourceAsync<Sprite>(res)).Object;
            await UniTask.Delay(TimeSpan.FromSeconds(.5f));
        }

        await UniTask.Delay(TimeSpan.FromSeconds(1.5f));

        foreach (var res in resources)
            provider.UnloadResource(res);

        await UniTask.Delay(TimeSpan.FromSeconds(1.5f));

        foreach (var res in resources)
            await provider.LoadResourceAsync<Sprite>(res);

        foreach (var res in resources)
        {
            SpriteRenderer.sprite = (await provider.LoadResourceAsync<Sprite>(res)).Object;
            await UniTask.Delay(TimeSpan.FromSeconds(.5f));
        }
    }

    private async UniTask TestTextureResources ()
    {
        foreach (var res in resources)
            await provider.LoadResourceAsync<Texture2D>(res);

        foreach (var res in resources)
        {
            var texture = (await provider.LoadResourceAsync<Texture2D>(res)).Object;
            SpriteRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * .5f);
            await UniTask.Delay(TimeSpan.FromSeconds(.5f));
        }
    }

    private async UniTask TestTextureByDir ()
    {
        var resources = await provider.LoadResourcesAsync<Texture2D>("Sprites");

        foreach (var res in resources)
        {
            var texture = res.Object;
            SpriteRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * .5f);
            await UniTask.Delay(TimeSpan.FromSeconds(.5f));
        }
    }

    private async UniTask TestNullPropagation ()
    {
        var loader = new ResourceLoader<Texture2D>(new List<IResourceProvider> { provider }, "Sprites");
        var image = await loader.LoadAsync("Image09");
        print(image);
        print("Propagated!");
    }
}
