using UnityEngine;
using UnityEngine.UI;

public class ActivateSceneByInput : ScriptableUIComponent<Text>
{
    private SceneLoader sceneLoader;

    protected override void Awake ()
    {
        base.Awake();
        sceneLoader = Context.Resolve<SceneLoader>(assertResult: true);
    }

    protected override void Start ()
    {
        base.Start();
        gameObject.SetActive(sceneLoader.ManualActivation);
    }

    private void Update ()
    {
        if (sceneLoader.IsReadyToActivate && Input.anyKeyDown)
        {
            sceneLoader.ActivateLoadedScene();
            gameObject.SetActive(false);
        }

        var opacity = sceneLoader.IsReadyToActivate ? (Mathf.Sin(Time.time) + 1.25f) / 2f : 0f;
        UIComponent.SetOpacity(opacity);
    }
}
