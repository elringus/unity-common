using UnityEngine;
using UnityEngine.SceneManagement;

public class TestLoadScene : MonoBehaviour
{
    public string Scene1Name;
    public string Scene2Name;

    [ContextMenu("Load Scene 1")]
    public void LoadScene1 ()
    {
        SceneManager.LoadScene(Scene1Name);
    }

    [ContextMenu("Load Scene 2")]
    public void LoadScene2 ()
    {
        SceneManager.LoadScene(Scene2Name);
    }
}
