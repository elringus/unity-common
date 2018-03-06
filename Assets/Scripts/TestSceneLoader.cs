using UnityEngine;

public class TestSceneLoader : MonoBehaviour
{
    public string SceneToLoadPath;
    public string TransitionScenePath;

    public void OnEnable ()
    {
        new SceneLoader(TransitionScenePath, true, true).LoadScene(SceneToLoadPath).Then(() => print("Load Complete!"));
    }
}
