// Copyright 2014-2017 Artyom Sovetnikov. All Rights Reserved.

namespace UnityCommon
{
// Copyright 2014-2017 Artyom Sovetnikov. All Rights Reserved.

namespace UnityCommon
{
using UnityEngine.UI;

public abstract class ScriptableSlider : ScriptableUIControl<Slider>
{
    protected override void BindUIEvents ()
    {
        UIComponent.onValueChanged.AddListener(OnValueChanged);
    }

    protected override void UnbindUIEvents ()
    {
        UIComponent.onValueChanged.RemoveListener(OnValueChanged);
    }

    protected abstract void OnValueChanged (float value);
}

}

}
