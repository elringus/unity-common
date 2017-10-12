using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Updater : MonoBehaviour
{
    private UnityAction[] actions = new UnityAction[0];

    private void Update ()
    {
        var length = actions.Length;
        for (int i = 0; i < length; i++)
            actions[i].Invoke();
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
