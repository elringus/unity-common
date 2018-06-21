using System;
using UnityEngine.UI;

namespace UnityCommon
{
    public class ScriptableSlider : ScriptableUIControl<Slider>
    {
        public event Action<float> OnSliderValueChanged;

        protected override void BindUIEvents ()
        {
            UIComponent.onValueChanged.AddListener(OnValueChanged);
            UIComponent.onValueChanged.AddListener(OnSliderValueChanged.SafeInvoke);
        }

        protected override void UnbindUIEvents ()
        {
            UIComponent.onValueChanged.RemoveListener(OnValueChanged);
            UIComponent.onValueChanged.RemoveListener(OnSliderValueChanged.SafeInvoke);
        }

        protected virtual void OnValueChanged (float value) { }
    }
}
