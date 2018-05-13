using System;
using UnityEngine.UI;

public class ScriptableDropdown : ScriptableUIControl<Dropdown>
{
    public event Action<int> OnDropdownValueChanged;

    protected override void BindUIEvents ()
    {
        UIComponent.onValueChanged.AddListener(OnValueChanged);
        UIComponent.onValueChanged.AddListener(OnDropdownValueChanged.SafeInvoke);
    }

    protected override void UnbindUIEvents ()
    {
        UIComponent.onValueChanged.RemoveListener(OnValueChanged);
        UIComponent.onValueChanged.RemoveListener(OnDropdownValueChanged.SafeInvoke);
    }

    protected virtual void OnValueChanged (int value) { }
}
