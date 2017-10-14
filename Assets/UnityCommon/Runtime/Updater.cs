using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Updater : MonoBehaviour
{
    public float UpdateDelay { get { return _updateDelay; } set { _updateDelay = value; } }

    [SerializeField] private float _updateDelay = 0f;

    private UnityAction[] actions = new UnityAction[0];
    private float lastUpdateTime = 0f;

    private void Update ()
    {
        var timeSinceLastUpdate = Time.time - lastUpdateTime;
        if (timeSinceLastUpdate < UpdateDelay) return;

        var length = actions.Length;
        for (int i = 0; i < length; i++)
            actions[i].Invoke();

        lastUpdateTime = Time.time;
    }

    private void OnDestroy ()
    {
        actions = new UnityAction[0];
    }

    public void AddAction (UnityAction action)
    {
        actions = actions.Append(action);
    }

    public void RemoveAction (UnityAction action)
    {
        actions = actions.Except(new UnityAction[1] { action }).ToArray();
    }
}
