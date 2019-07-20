using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityCommon
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ScriptableGridSlot : ScriptableUIBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    { 
        public class Constructor<TSlot> where TSlot : ScriptableGridSlot
        {
            public readonly TSlot ConstructedSlot;

            public Constructor (TSlot prototype, string id, OnClicked onClicked = null)
            {
                ConstructedSlot = Instantiate(prototype);
                ConstructedSlot.Id = id;
                ConstructedSlot.onClickedAction = onClicked;
            }
        }

        /// <summary>
        /// Action to invoke when the slot body is clicked by the user.
        /// </summary>
        /// <param name="slotId">ID of the clicked slot.</param>
        public delegate void OnClicked (string slotId);

        public string Id { get; private set; }
        public int NumberInGrid => transform.GetSiblingIndex() + 1;

        [SerializeField] private float hoverOpacityFade = .25f;

        private Tweener<FloatTween> fadeTweener;
        private OnClicked onClickedAction;

        protected override void Awake ()
        {
            base.Awake();

            fadeTweener = new Tweener<FloatTween>(this);
        }

        protected override void Start ()
        {
            base.Start();

            SetOpacity(1 - hoverOpacityFade);
        }

        public virtual void OnPointerEnter (PointerEventData eventData)
        {
            if (fadeTweener.IsRunning) fadeTweener.CompleteInstantly();
            var tween = new FloatTween(CurrentOpacity, 1f, FadeTime, SetOpacity, true);
            fadeTweener.Run(tween);
        }

        public virtual void OnPointerExit (PointerEventData eventData)
        {
            if (fadeTweener.IsRunning) fadeTweener.CompleteInstantly();
            var tween = new FloatTween(CurrentOpacity, 1f - hoverOpacityFade, FadeTime, SetOpacity, true);
            fadeTweener.Run(tween);
        }

        public virtual void OnPointerClick (PointerEventData eventData)
        {
            onClickedAction?.Invoke(Id);
        }
    }
}
