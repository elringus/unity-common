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
    };

    private void Awake ()
    {
        //InitializeProjectResourceProvider();
        InitializeGoogleDriveResourceProvider();
    }

    private IEnumerator Start ()
    {
        yield return ResolveTextByPath();
        yield return ResolveTextByPath();
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
        provider.AddConverter(new PngToSpriteConverter());
        provider.AddConverter(new TxtToStringConverter());
    }

    private IEnumerator ResolveSpritesByPath ()
    {
        provider = Context.Resolve<IResourceProvider>();
        var loadAllAction = provider.LoadResources<Sprite>("Sprites");

        yield return loadAllAction;

        foreach (var spriteResource in loadAllAction.State)
        {
            SpriteRenderer.sprite = spriteResource.Object;
            yield return new WaitForSeconds(1);
        }

        foreach (var spriteResource in loadAllAction.State)
            provider.UnloadResource(spriteResource.Path);

        yield return new WaitForSeconds(3);
    }

    private IEnumerator ResolveTextByPath ()
    {
        provider = Context.Resolve<IResourceProvider>();
        var loadAllAction = provider.LoadResources<string>("Text");

        yield return loadAllAction;

        foreach (var textResource in loadAllAction.State)
        {
            text = textResource.Object;
            yield return new WaitForSeconds(1);
        }

        foreach (var textResource in loadAllAction.State)
            provider.UnloadResource(textResource.Path);

        yield return new WaitForSeconds(3);
    }

    private IEnumerator ResolveByFullPath ()
    {
        provider = Context.Resolve<IResourceProvider>();
        var waitFordelay = new WaitForSeconds(3);

        yield return waitFordelay;

        foreach (var res in RESOURCES)
            provider.LoadResource<Sprite>(res);

        while (provider.IsLoading) yield return null;

        foreach (var res in RESOURCES)
            SpriteRenderer.sprite = provider.LoadResource<Sprite>(res).State.Object;

        yield return waitFordelay;

        foreach (var res in RESOURCES)
            provider.UnloadResource(res);

        yield return waitFordelay;

        foreach (var res in RESOURCES)
            provider.LoadResource<Sprite>(res);

        while (provider.IsLoading) yield return null;

        foreach (var res in RESOURCES)
            SpriteRenderer.sprite = provider.LoadResource<Sprite>(res).State.Object;
    }
}
