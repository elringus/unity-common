using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UnityCommon
{
    public class ScriptableUIBehaviour : UIBehaviour
    {
        [System.Serializable]
        private class VisibilityChangedEvent : UnityEvent<bool> { }

        public event Action<bool> OnVisibilityChanged;

        public float FadeTime { get => fadeTime; set => fadeTime = value; }
        public bool VisibleOnAwake => visibleOnAwake; 
        public virtual bool Visible { get => visible; set => SetVisibility(value); }
        public virtual float CurrentOpacity => GetCurrentOpacity();
        public virtual bool Interactable => CanvasGroup ? CanvasGroup.interactable : true;
        public RectTransform RectTransform => GetRectTransform();
        public Canvas TopmostCanvas => ObjectUtils.IsValid(topmostCanvasCache) ? topmostCanvasCache : (topmostCanvasCache = FindTopmostCanvas());
        public int SortingOrder { get => ObjectUtils.IsValid(TopmostCanvas) ? TopmostCanvas.sortingOrder : 0; set => SetSortingOrder(value); }
        public RenderMode RenderMode { get => ObjectUtils.IsValid(TopmostCanvas) ? TopmostCanvas.renderMode : default; set => SetRenderMode(value); }
        public Camera RenderCamera { get => ObjectUtils.IsValid(TopmostCanvas) ? TopmostCanvas.worldCamera : null; set => SetRenderCamera(value); }

        protected CanvasGroup CanvasGroup { get; private set; }

        [Tooltip("Whether to permamently disable interaction with the object, no matter the visibility.")]
        [SerializeField] private bool disableInteraction = false;
        [Tooltip("Whether UI element should be visible or hidden on awake.")]
        [SerializeField] private bool visibleOnAwake = true;
        [Tooltip("Fade duration (in seconds) when changing visiblity.")]
        [SerializeField] private float fadeTime = .3f;
        [Tooltip("When assigned, will make the object focused (for keyboard or gamepad control) when the UI becomes visible.")]
        [SerializeField] private GameObject focusObject = default;
        [Tooltip("Invoked when visibility of the UI is changed.")]
        [SerializeField] private VisibilityChangedEvent onVisibilityChanged = default;

        private Tweener<FloatTween> fadeTweener;
        private RectTransform rectTransform;
        private Canvas topmostCanvasCache;
        private bool visible;

        public virtual async Task SetVisibilityAsync (bool visible, float? fadeTime = null)
        {
            if (fadeTweener.IsRunning)
                fadeTweener.Stop();

            this.visible = visible;

            HandleVisibilityChanged(visible);

            if (!CanvasGroup) return;

            if (!disableInteraction)
            {
                CanvasGroup.interactable = visible;
                CanvasGroup.blocksRaycasts = visible;
            }

            var fadeDuration = fadeTime ?? FadeTime;
            var targetOpacity = visible ? 1f : 0f;

            if (fadeDuration == 0f)
            {
                CanvasGroup.alpha = targetOpacity;
                return;
            }

            var tween = new FloatTween(CanvasGroup.alpha, targetOpacity, fadeDuration, alpha => CanvasGroup.alpha = alpha);
            await fadeTweener.RunAsync(tween);
        }

        public virtual void SetVisibility (bool visible)
        {
            if (fadeTweener.IsRunning)
                fadeTweener.Stop();

            this.visible = visible;

            HandleVisibilityChanged(visible);

            if (!CanvasGroup) return;

            if (!disableInteraction)
            {
                CanvasGroup.interactable = visible;
                CanvasGroup.blocksRaycasts = visible;
            }

            CanvasGroup.alpha = visible ? 1f : 0f;
        }

        public virtual void ToggleVisibility ()
        {
            SetVisibilityAsync(!Visible).WrapAsync();
        }

        public virtual void Show ()
        {
            if (Visible) return;
            SetVisibilityAsync(true).WrapAsync();
        }

        public virtual void Hide ()
        {
            if (!Visible) return;
            SetVisibilityAsync(false).WrapAsync();
        }

        public virtual float GetCurrentOpacity ()
        {
            if (CanvasGroup) return CanvasGroup.alpha;
            return 1f;
        }

        public virtual void SetOpacity (float opacity)
        {
            if (!CanvasGroup) return;

            CanvasGroup.alpha = opacity;
        }

        public virtual void SetInteractable (bool interactable)
        {
            if (!CanvasGroup) return;

            CanvasGroup.interactable = interactable;
        }

        public void ClearFocus ()
        {
            if (EventSystem.current &&
                EventSystem.current.currentSelectedGameObject &&
                EventSystem.current.currentSelectedGameObject.transform.IsChildOf(transform))
                EventSystem.current.SetSelectedGameObject(null);
        }

        public void SetFocus ()
        {
            if (EventSystem.current)
                EventSystem.current.SetSelectedGameObject(gameObject);
        }

        public void SetFont (Font font)
        {
            if (!ObjectUtils.IsValid(font)) return;

            foreach (var text in GetComponentsInChildren<UnityEngine.UI.Text>(true))
                text.font = font;

            #if TMPRO_AVAILABLE
            var fontAsset = TMPro.TMP_FontAsset.CreateFontAsset(font);
            if (!ObjectUtils.IsValid(fontAsset)) return;
            DontDestroyOnLoad(fontAsset);
            fontAsset.hideFlags = HideFlags.HideAndDontSave;
            foreach (var text in GetComponentsInChildren<TMPro.TextMeshProUGUI>(true))
                text.font = fontAsset;
            #endif
        }

        public void SetFontSize (int size)
        {
            foreach (var text in GetComponentsInChildren<UnityEngine.UI.Text>(true))
                text.fontSize = size;

            #if TMPRO_AVAILABLE
            foreach (var text in GetComponentsInChildren<TMPro.TextMeshProUGUI>(true))
                text.fontSize = size;
            #endif
        }

        protected override void Awake ()
        {
            base.Awake();

            fadeTweener = new Tweener<FloatTween>(this);
            CanvasGroup = GetComponent<CanvasGroup>();

            if (CanvasGroup && disableInteraction)
            {
                CanvasGroup.interactable = false;
                CanvasGroup.blocksRaycasts = false;
            }

            SetVisibility(VisibleOnAwake);
        }

        /// <summary>
        /// Invoked when visibility of the UI is changed.
        /// </summary>
        /// <param name="visible">The new visibility of the UI.</param>
        protected virtual void HandleVisibilityChanged (bool visible)
        {
            OnVisibilityChanged?.Invoke(visible);
            onVisibilityChanged?.Invoke(visible);

            if (focusObject && visible && EventSystem.current)
                EventSystem.current.SetSelectedGameObject(focusObject);
        }

        private RectTransform GetRectTransform ()
        {
            if (!rectTransform)
                rectTransform = GetComponent<RectTransform>();
            return rectTransform;
        }

        private Canvas FindTopmostCanvas ()
        {
            var parentCanvases = gameObject.GetComponentsInParent<Canvas>();
            if (parentCanvases != null && parentCanvases.Length > 0)
                return parentCanvases[parentCanvases.Length - 1];
            return null;
        }

        private void SetSortingOrder (int value)
        {
            if (!ObjectUtils.IsValid(TopmostCanvas)) return;
            TopmostCanvas.sortingOrder = value;
        }

        private void SetRenderMode (RenderMode renderMode)
        {
            if (!ObjectUtils.IsValid(TopmostCanvas)) return;
            TopmostCanvas.renderMode = renderMode;
        }

        private void SetRenderCamera (Camera camera)
        {
            if (!ObjectUtils.IsValid(TopmostCanvas)) return;
            TopmostCanvas.worldCamera = camera;
        }
    }
}
