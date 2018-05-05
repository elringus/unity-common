using UnityEngine;

public class ApplicationBehaviour : MonoBehaviour
{
    public static ApplicationBehaviour Singleton => singleton ?? CreateSingleton();

    private static ApplicationBehaviour singleton;

    private static ApplicationBehaviour CreateSingleton ()
    {
        singleton = new GameObject("ApplicationBehaviour").AddComponent<ApplicationBehaviour>();
        singleton.gameObject.hideFlags = HideFlags.DontSave;
        DontDestroyOnLoad(singleton.gameObject);
        return singleton;
    }
}
