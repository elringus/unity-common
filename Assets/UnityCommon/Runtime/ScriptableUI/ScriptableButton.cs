using UnityEngine.UI;

public abstract class ScriptableButton : ScriptableUIControl<Button>
{
    protected override void BindUIEvents ()
    {
        UIComponent.onClick.AddListener(OnButtonClick);
    }

    protected override void UnbindUIEvents ()
    {
        UIComponent.onClick.RemoveListener(OnButtonClick);
    }

    protected abstract void OnButtonClick ();
}
