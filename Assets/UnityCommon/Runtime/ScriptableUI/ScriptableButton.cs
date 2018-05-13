using System;
using UnityEngine.UI;

public class ScriptableButton : ScriptableUIControl<Button>
{
    public event Action OnButtonClicked;

    protected override void BindUIEvents ()
    {
        UIComponent.onClick.AddListener(OnButtonClick);
        UIComponent.onClick.AddListener(OnButtonClicked.SafeInvoke);
    }

    protected override void UnbindUIEvents ()
    {
        UIComponent.onClick.RemoveListener(OnButtonClick);
        UIComponent.onClick.RemoveListener(OnButtonClicked.SafeInvoke);
    }

    protected virtual void OnButtonClick () { }
}
