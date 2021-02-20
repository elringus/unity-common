using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UnityCommon
{
    [RequireComponent(typeof(CanvasGroup)), RequireComponent(typeof(UnityEngine.UI.Button))]
    public class ScriptableGridSlot : ScriptableButton, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        [Serializable]
        private class OnSlotClickedEvent : UnityEvent<string> { }

        public virtual string Id { get; }

        [Tooltip("Opacity to fade to when the slot is hovered or selected; set to zero to disable the fade behaviour.")]
        [SerializeField] private float hoverOpacityFade = .25f;
        [SerializeField] private OnSlotClickedEvent onSlotClicked = default;

        private readonly Tweener<FloatTween> fadeTweener = new Tweener<FloatTween>();

        public virtual void OnPointerEnter (PointerEventData eventData) => FadeInSlot();

        public virtual void OnPointerExit (PointerEventData eventData) => FadeOutSlot();

        public virtual void OnSelect (BaseEventData eventData) => FadeInSlot();

        public virtual void OnDeselect (BaseEventData eventData) => FadeOutSlot();

        protected override void Start ()
        {
            base.Start();

            if (hoverOpacityFade > 0)
                SetOpacity(1 - hoverOpacityFade);
        }

        protected override void OnButtonClick ()
        {
            base.OnButtonClick();

            onSlotClicked?.Invoke(Id);
        }

        protected virtual void FadeInSlot ()
        {
            if (hoverOpacityFade <= 0) return;
            if (fadeTweener.Running) fadeTweener.CompleteInstantly();
            var tween = new FloatTween(Opacity, 1f, FadeTime, SetOpacity, true, target: this);
            fadeTweener.Run(tween);
        }

        protected virtual void FadeOutSlot ()
        {
            if (hoverOpacityFade <= 0) return;
            if (fadeTweener.Running) fadeTweener.CompleteInstantly();
            var tween = new FloatTween(Opacity, 1f - hoverOpacityFade, FadeTime, SetOpacity, true, target: this);
            fadeTweener.Run(tween);
        }
    }
}
