using UnityEngine.UI;

public abstract class ScriptableDropdown : ScriptableUIControl<Dropdown>
{
    protected override void BindUIEvents ()
    {
        UIComponent.onValueChanged.AddListener(OnValueChanged);
    }

    protected override void UnbindUIEvents ()
    {
        UIComponent.onValueChanged.RemoveListener(OnValueChanged);
    }

    protected abstract void OnValueChanged (int value);
}
