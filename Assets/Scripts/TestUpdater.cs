using UnityCommon;
using UnityEngine;

[RequireComponent(typeof(Updater))]
public class TestUpdater : MonoBehaviour
{
    private Updater updater;

    private void Awake ()
    {
        updater = GetComponent<Updater>();
    }

    private void OnEnable ()
    {
        updater.AddAction(Action1);
        updater.AddAction(Action2);
        updater.AddAction(Action3);
        updater.AddAction(Action4);
    }

    private void OnDisable ()
    {
        updater.RemoveAction(Action1);
        updater.RemoveAction(Action2);
        updater.RemoveAction(Action3);
        updater.RemoveAction(Action4);
    }

    private void Action1 () => print("Action1");
    private void Action2 () => print("Action2");
    private void Action3 () => print("Action3");
    private void Action4 () => print("Action4");
}
