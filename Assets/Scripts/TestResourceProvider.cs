using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestResourceProvider : MonoBehaviour
{
    public SpriteRenderer SpriteRenderer;

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
        yield return TestResourcExists();
    }

    private void OnGUI ()
    {
        GUILayout.TextArea(text);

        if (provider != null && provider.IsLoading)
            GUILayout.Label(provider.LoadProgress.ToString());
    }

    private void InitializeProjectResourceProvider ()
    {
        var provider = Context.Resolve<ProjectResourceProvider>();
        provider.AddRedirector(new TextAssetToStringConverter());
    }

    private void InitializeGoogleDriveResourceProvider ()
    {
        var provider = Context.Resolve<GoogleDriveResourceProvider>();
        provider.DriveRootPath = "Resources";
        provider.ConcurrentRequestsLimit = 2;
        provider.AddConverter(new JpgOrPngToSpriteConverter());
        provider.AddConverter(new GDocToStringConverter());
        provider.AddConverter(new GFolderToFolderConverter());
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
