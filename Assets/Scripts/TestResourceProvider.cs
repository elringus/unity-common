using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestResourceProvider : MonoBehaviour
{
    public SpriteRenderer SpriteRenderer;

    private readonly List<string> RESOURCES = new List<string>() {
        "Sprites/Image01",
        "Sprites/Image02",
        "Sprites/Image03",
    };

    private void Awake ()
    {
        Context.Resolve<ProjectResourceProvider>();
    }

    private IEnumerator Start ()
    {
        var provider = Context.Resolve<IResourceProvider>();
        var waitFordelay = new WaitForSeconds(3);

        yield return waitFordelay;

        foreach (var res in RESOURCES)
            provider.LoadResourceAsync<Sprite>(res);

        while (provider.IsLoading) yield return null;

        foreach (var res in RESOURCES)
            SpriteRenderer.sprite = provider.GetResource<Sprite>(res);

        yield return waitFordelay;

        foreach (var res in RESOURCES)
            provider.UnloadResourceAsync(res);

        yield return waitFordelay;

        foreach (var res in RESOURCES)
            provider.LoadResourceAsync<Sprite>(res);

        while (provider.IsLoading) yield return null;

        foreach (var res in RESOURCES)
            SpriteRenderer.sprite = provider.GetResource<Sprite>(res);
    }
}
