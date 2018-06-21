using UnityEngine;
using UnityEngine.UI;

namespace UnityCommon
{
    public class LabeledButton : Button
    {
        [SerializeField] private Text labelText;
        [SerializeField] private ColorBlock labelColors = ColorBlock.defaultColorBlock;

        private Tweener<ColorTween> tintTweener;

        protected override void Awake ()
        {
            base.Awake();

            tintTweener = new Tweener<ColorTween>(this);
        }

        #if UNITY_EDITOR
        protected override void OnValidate ()
        {
            base.OnValidate();

            if (!labelText) labelText = GetComponentInChildren<Text>();
        }
        #endif

        protected override void Start ()
        {
            base.Start();

            if (!labelText) labelText = GetComponentInChildren<Text>();
        }

        protected override void DoStateTransition (SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);

            if (!labelText) return;

            Color tintColor;
            switch (state)
            {
                case SelectionState.Normal:
                    tintColor = labelColors.normalColor;
                    break;
                case SelectionState.Highlighted:
                    tintColor = labelColors.highlightedColor;
                    break;
                case SelectionState.Pressed:
                    tintColor = labelColors.pressedColor;
                    break;
                case SelectionState.Disabled:
                    tintColor = labelColors.disabledColor;
                    break;
                default:
                    tintColor = Color.black;
                    break;
            }

            if (instant)
            {
                if (tintTweener != null && tintTweener.IsRunning) tintTweener.CompleteInstantly();
                labelText.color = tintColor;
            }
            else if (tintTweener != null)
            {
                var tween = new ColorTween(labelText.color, tintColor * labelColors.colorMultiplier, ColorTweenMode.All, labelColors.fadeDuration, c => labelText.color = c);
                tintTweener.Run(tween);
            }
        }
    }
}
