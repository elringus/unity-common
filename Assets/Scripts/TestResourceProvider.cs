using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestResourceProvider : MonoBehaviour
{
    public SpriteRenderer SpriteRenderer;

    private IResourceProvider provider;

    private readonly List<string> RESOURCES = new List<string>() {
        "Sprites/Image01",
        "Sprites/Image02",
        "Sprites/Image03",
    };

    private void Awake ()
    {
        var provider = Context.Resolve<GoogleDriveResourceProvider>();
        provider.DriveRootPath = "Resources";
        provider.AddConverter(new PngToSpriteConverter());
        //Context.Resolve<ProjectResourceProvider>();
    }

    private IEnumerator Start ()
    {
        provider = Context.Resolve<IResourceProvider>();
        var waitFordelay = new WaitForSeconds(3);

        yield return waitFordelay;

        foreach (var res in RESOURCES)
            provider.LoadResource<Sprite>(res);

        while (provider.IsLoading) yield return null;

        foreach (var res in RESOURCES)
            SpriteRenderer.sprite = provider.LoadResource<Sprite>(res).Object;

        yield return waitFordelay;

        foreach (var res in RESOURCES)
            provider.UnloadResource(res);

        yield return waitFordelay;

        foreach (var res in RESOURCES)
            provider.LoadResource<Sprite>(res);

        while (provider.IsLoading) yield return null;

        foreach (var res in RESOURCES)
            SpriteRenderer.sprite = provider.LoadResource<Sprite>(res).Object;
    }

    private void OnGUI ()
    {
        if (provider != null && provider.IsLoading)
            GUILayout.Label(provider.LoadProgress.ToString());
    }
}
