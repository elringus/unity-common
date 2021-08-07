using UnityEngine;
using UnityEngine.UI;

namespace UnityCommon
{
    public class LabeledButton : Button
    {
        public virtual Text Label => labelText ? labelText : (labelText = GetComponentInChildren<Text>());
        public virtual ColorBlock LabelColorBlock => labelColors;
        public virtual Color LabelColorMultiplier
        {
            get => labelColorMultiplier; 
            set { labelColorMultiplier = value; DoStateTransition(currentSelectionState, false); }
        }

        [SerializeField] private Text labelText = default;
        [SerializeField] private ColorBlock labelColors = ColorBlock.defaultColorBlock;

        private Color labelColorMultiplier = Color.white;
        private Tweener<ColorTween> tintTweener;

        protected override void Awake ()
        {
            base.Awake();

            tintTweener = new Tweener<ColorTween>();
        }

        protected override void DoStateTransition (SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);

            if (!Label) return;

            Color tintColor;
            switch (state)
            {
                case SelectionState.Normal:
                    tintColor = LabelColorBlock.normalColor;
                    break;
                case SelectionState.Highlighted:
                    tintColor = LabelColorBlock.highlightedColor;
                    break;
                case SelectionState.Pressed:
                    tintColor = LabelColorBlock.pressedColor;
                    break;
                case SelectionState.Selected:
                    tintColor = LabelColorBlock.selectedColor;
                    break;
                case SelectionState.Disabled:
                    tintColor = LabelColorBlock.disabledColor;
                    break;
                default:
                    tintColor = Color.white;
                    break;
            }

            if (instant)
            {
                if (tintTweener != null && tintTweener.Running) tintTweener.CompleteInstantly();
                Label.color = tintColor * LabelColorBlock.colorMultiplier * LabelColorMultiplier;
            }
            else if (tintTweener != null)
            {
                var tween = new ColorTween(Label.color, tintColor * LabelColorBlock.colorMultiplier * LabelColorMultiplier, ColorTweenMode.All, LabelColorBlock.fadeDuration, c => Label.color = c);
                tintTweener.Run(tween, target: Label);
            }
        }
    }
}
