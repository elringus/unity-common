using System;
using UnityEngine;
using UnityEngine.UI;

namespace UnityCommon
{
    public class ScriptableScrollRect : ScriptableUIControl<ScrollRect>
    {
        public event Action<Vector2> OnPositionChanged;

        protected override void BindUIEvents ()
        {
            UIComponent.onValueChanged.AddListener(OnScrollPositionChanged);
            UIComponent.onValueChanged.AddListener(OnPositionChanged.SafeInvoke);
        }

        protected override void UnbindUIEvents ()
        {
            UIComponent.onValueChanged.RemoveListener(OnScrollPositionChanged);
            UIComponent.onValueChanged.RemoveListener(OnPositionChanged.SafeInvoke);
        }

        protected virtual void OnScrollPositionChanged (Vector2 scrollPosition) { }
    }
}
