using UnityEngine;
using UnityEngine.UI;

public abstract class ScriptableScrollRect : ScriptableUIControl<ScrollRect>
{
    protected override void BindUIEvents ()
    {
        UIComponent.onValueChanged.AddListener(OnScrollPositionChanged);
    }

    protected override void UnbindUIEvents ()
    {
        UIComponent.onValueChanged.RemoveListener(OnScrollPositionChanged);
    }

    protected abstract void OnScrollPositionChanged (Vector2 scrollPosition);
}
