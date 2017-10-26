using UnityEngine;

public class TestResourceProvider : MonoBehaviour
{
    private void Awake ()
    {
        Context.Resolve<ProjectResourceProvider>();
    }

    private void Start ()
    {
        print(Context.Resolve<IResourceProvider>());
    }
}
