using System;
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

    private void OnApplicationQuit ()
    {
        actions = null;
    }

    public void AddAction (UnityAction action)
    {
        Array.Resize(ref actions, actions.Length + 1);
        actions[actions.Length - 1] = action;
    }
}
