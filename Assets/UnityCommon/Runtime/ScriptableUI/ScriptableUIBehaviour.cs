using System;
using UniRx.Async;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UnityCommon
{
    /// <summary>
    /// A wrapper over <see cref="UIBehaviour"/> providing various scripting utility APIs.
    /// </summary>
    public class ScriptableUIBehaviour : UIBehaviour
    {
        public enum FocusMode
        {
            Visibility,
            Navigation
        }

        /// <summary>
        /// Event invoked when visibility of the UI changes.
        /// </summary>
        public event Action<bool> OnVisibilityChanged;

        /// <summary>
        /// Fade duration (in seconds) when changing visibility of the UI;
        /// requires a <see cref="UnityEngine.CanvasGroup"/> on the same game object.
        /// </summary>
        public virtual float FadeTime { get => fadeTime; set => fadeTime = value; }
        /// <summary>
        /// Whether to ignore time scale when changing visibility (fade animation).
        /// </summary>
        public virtual bool IgnoreTimeScale { get => ignoreTimeScale; set => ignoreTimeScale = value; }
        /// <summary>
        /// Whether the UI element should be visible or hidden on awake.
        /// requires a <see cref="UnityEngine.CanvasGroup"/> on the same game object.
        /// </summary>
        public virtual bool VisibleOnAwake => visibleOnAwake;
        /// <summary>
        /// Determines when to focus the object: on the UI becomes visible or on first navigation attempt (arrow keys or d-pad) while the UI is visible.
        /// </summary>
        public virtual FocusMode FocusModeType { get => focusMode; set => focusMode = value; }
        /// <summary>
        /// The object to focus (for keyboard or gamepad control) when the UI becomes visible or upon navigation.
        /// </summary>
        public virtual GameObject FocusObject { get => focusObject; set => focusObject = value; }
        /// <summary>
        /// Whether the UI is currently visible.
        /// requires a <see cref="UnityEngine.CanvasGroup"/> on the same game object.
        /// </summary>
        public virtual bool Visible { get => visible; set => SetVisibility(value); }
        /// <summary>
        /// Current opacity (alpha) of the UI element, in 0.0 to 1.0 range.
        /// requires a <see cref="UnityEngine.CanvasGroup"/> on the same game object, will always return 1.0 otherwise.
        /// </summary>
        public virtual float Opacity => CanvasGroup ? CanvasGroup.alpha : 1f;
        /// <summary>
        /// Whether the UI is currently interactable.
        /// requires a <see cref="UnityEngine.CanvasGroup"/> on the same game object.
        /// </summary>
        public virtual bool Interactable { get => !CanvasGroup || CanvasGroup.interactable; set => SetInteractable(value); }
        /// <summary>
        /// Transform used by the UI element.
        /// </summary>
        public virtual RectTransform RectTransform => GetRectTransform();
        /// <summary>
        /// Topmost parent (in the game object hierarchy) canvas component.
        /// </summary>
        public virtual Canvas TopmostCanvas => ObjectUtils.IsValid(topmostCanvasCache) ? topmostCanvasCache : (topmostCanvasCache = gameObject.FindTopmostComponent<Canvas>());
        /// <summary>
        /// Current sort order of the UI element, as per <see cref="TopmostCanvas"/>.
        /// </summary>
        public virtual int SortingOrder { get => ObjectUtils.IsValid(TopmostCanvas) ? TopmostCanvas.sortingOrder : 0; set => SetSortingOrder(value); }
        /// <summary>
        /// Current render mode of the UI element, as per <see cref="TopmostCanvas"/>.
        /// </summary>
        public virtual RenderMode RenderMode { get => ObjectUtils.IsValid(TopmostCanvas) ? TopmostCanvas.renderMode : default; set => SetRenderMode(value); }
        /// <summary>
        /// Current render camera of the UI element, as per <see cref="TopmostCanvas"/>.
        /// </summary>
        public virtual Camera RenderCamera { get => ObjectUtils.IsValid(TopmostCanvas) ? TopmostCanvas.worldCamera : null; set => SetRenderCamera(value); }

        protected static GameObject FocusOnNavigation { get; set; }

        protected virtual CanvasGroup CanvasGroup { get; private set; }
        protected virtual bool ControlOpacity => controlOpacity;

        [Tooltip("Whether to permanently disable interaction with the object, no matter the visibility. Requires `Canvas Group` component on the same game object.")]
        [SerializeField] private bool disableInteraction = false;
        [Tooltip("Whether UI element should be visible or hidden on awake.")]
        [SerializeField] private bool visibleOnAwake = true;
        [Tooltip("Whether to change opacity (alpha) of Canvas Group in correspondence to visibility of the UI element. Requires `Canvas Group` component on the same game object.")]
        [SerializeField] private bool controlOpacity = true;
        [Tooltip("When `Control Opacity` is enabled, controls opacity fade duration (in seconds) when changing visibility.")]
        [SerializeField] private float fadeTime = .3f;
        [Tooltip("When `Control Opacity` is enabled, controls whether to ignore time scale when changing visibility.")]
        [SerializeField] private bool ignoreTimeScale = true;
        [Tooltip("When assigned, will make the object focused (for keyboard or gamepad control) when the UI becomes visible or upon navigation.")]
        [SerializeField] private GameObject focusObject = default;
        [Tooltip("When `Focus Object` is assigned, determines when to focus the object: on the UI becomes visible or on first navigation attempt (arrow keys or d-pad) while the UI is visible. Be aware, that gamepad support for Navigation mode requires Unity's new input system package installed.")]
        [SerializeField] private FocusMode focusMode = default;
        [Tooltip("Invoked when the UI element is shown.")]
        [SerializeField] private UnityEvent onShow = default;
        [Tooltip("Invoked when the UI element is hidden.")]
        [SerializeField] private UnityEvent onHide = default;

        private readonly Tweener<FloatTween> fadeTweener = new Tweener<FloatTween>();
        private RectTransform rectTransform;
        private Canvas topmostCanvasCache;
        private bool visible;

        /// <summary>
        /// Gradually changes <see cref="Visible"/> with fade animation over <see cref="FadeTime"/> or specified time (in seconds).
        /// </summary>
        public virtual async UniTask ChangeVisibilityAsync (bool visible, float? duration = null, AsyncToken asyncToken = default)
        {
            if (fadeTweener.Running)
                fadeTweener.Stop();

            this.visible = visible;

            HandleVisibilityChanged(visible);

            if (!CanvasGroup) return;

            if (!disableInteraction)
            {
                CanvasGroup.interactable = visible;
                CanvasGroup.blocksRaycasts = visible;
            }

            if (!controlOpacity) return;

            var fadeDuration = duration ?? FadeTime;
            var targetOpacity = visible ? 1f : 0f;

            if (fadeDuration == 0f)
            {
                CanvasGroup.alpha = targetOpacity;
                return;
            }

            var tween = new FloatTween(CanvasGroup.alpha, targetOpacity, fadeDuration, SetOpacity, IgnoreTimeScale);
            await fadeTweener.RunAsync(tween, asyncToken, this);
        }

        /// <summary>
        /// Changes <see cref="Visible"/>.
        /// </summary>
        public virtual void SetVisibility (bool visible)
        {
            if (fadeTweener.Running)
                fadeTweener.Stop();

            this.visible = visible;

            HandleVisibilityChanged(visible);

            if (!CanvasGroup) return;

            if (!disableInteraction)
            {
                CanvasGroup.interactable = visible;
                CanvasGroup.blocksRaycasts = visible;
            }

            if (controlOpacity)
                CanvasGroup.alpha = visible ? 1f : 0f;
        }

        /// <summary>
        /// Toggles <see cref="Visible"/>.
        /// </summary>
        public virtual void ToggleVisibility ()
        {
            ChangeVisibilityAsync(!Visible).Forget();
        }

        /// <summary>
        /// Reveals the UI over <see cref="FadeTime"/>.
        /// </summary>
        [ContextMenu("Show")]
        public virtual void Show ()
        {
            ChangeVisibilityAsync(true).Forget();
        }

        /// <summary>
        /// Hides the UI over <see cref="FadeTime"/>.
        /// </summary>
        [ContextMenu("Hide")]
        public virtual void Hide ()
        {
            ChangeVisibilityAsync(false).Forget();
        }

        /// <summary>
        /// Changes <see cref="Opacity"/>; 
        /// has no effect when <see cref="CanvasGroup"/> is missing on the same game object.
        /// </summary>
        public virtual void SetOpacity (float opacity)
        {
            if (!CanvasGroup) return;
            CanvasGroup.alpha = opacity;
        }

        /// <summary>
        /// Changes <see cref="Interactable"/>; 
        /// has no effect when <see cref="CanvasGroup"/> is missing on the same game object.
        /// </summary>
        public virtual void SetInteractable (bool interactable)
        {
            if (!CanvasGroup) return;
            CanvasGroup.interactable = interactable;
        }

        /// <summary>
        /// Removes input focus from the UI element.
        /// </summary>
        public virtual void ClearFocus ()
        {
            if (EventSystem.current &&
                EventSystem.current.currentSelectedGameObject &&
                EventSystem.current.currentSelectedGameObject.transform.IsChildOf(transform))
                EventSystem.current.SetSelectedGameObject(null);
        }

        /// <summary>
        /// Applies input focus to the UI element.
        /// </summary>
        public virtual void SetFocus ()
        {
            if (EventSystem.current)
                EventSystem.current.SetSelectedGameObject(gameObject);
        }

        protected override void Awake ()
        {
            base.Awake();

            CanvasGroup = GetComponent<CanvasGroup>();

            if (CanvasGroup && disableInteraction)
            {
                CanvasGroup.interactable = false;
                CanvasGroup.blocksRaycasts = false;
            }

            SetVisibility(VisibleOnAwake);
        }

        protected virtual void Update ()
        {
            HandleNavigationFocus();
        }

        /// <summary>
        /// Invoked when visibility of the UI is changed.
        /// </summary>
        /// <param name="visible">The new visibility of the UI.</param>
        protected virtual void HandleVisibilityChanged (bool visible)
        {
            OnVisibilityChanged?.Invoke(visible);

            if (visible) onShow?.Invoke();
            else onHide?.Invoke();

            if (focusObject && visible && EventSystem.current)
                switch (focusMode)
                {
                    case FocusMode.Visibility:
                        EventSystem.current.SetSelectedGameObject(focusObject);
                        FocusOnNavigation = null;
                        break;
                    case FocusMode.Navigation:
                        FocusOnNavigation = focusObject;
                        break;
                }
        }

        protected virtual void HandleNavigationFocus ()
        {
            if (focusMode != FocusMode.Navigation || !FocusOnNavigation || !Visible || !EventSystem.current) return;

            if (SampleInputSystemNavigation() || SampleLegacyInputNavigation())
            {
                EventSystem.current.SetSelectedGameObject(FocusOnNavigation);
                FocusOnNavigation = null;
            }
        }

        protected virtual bool SampleInputSystemNavigation ()
        {
            #if ENABLE_INPUT_SYSTEM && INPUT_SYSTEM_AVAILABLE
            return UnityEngine.InputSystem.Gamepad.current is UnityEngine.InputSystem.Gamepad gamepad &&
                   (gamepad.leftStick.up.wasPressedThisFrame ||
                    gamepad.leftStick.down.wasPressedThisFrame ||
                    gamepad.leftStick.left.wasPressedThisFrame ||
                    gamepad.leftStick.right.wasPressedThisFrame ||
                    gamepad.dpad.up.wasPressedThisFrame ||
                    gamepad.dpad.down.wasPressedThisFrame ||
                    gamepad.dpad.left.wasPressedThisFrame ||
                    gamepad.dpad.right.wasPressedThisFrame) ||
                   // Generic HID gamepads are detected as joysticks by the input system.
                   UnityEngine.InputSystem.Joystick.current is UnityEngine.InputSystem.Joystick joystick &&
                   (joystick.stick.up.wasPressedThisFrame ||
                    joystick.stick.down.wasPressedThisFrame ||
                    joystick.stick.left.wasPressedThisFrame ||
                    joystick.stick.right.wasPressedThisFrame) ||
                   UnityEngine.InputSystem.Keyboard.current is UnityEngine.InputSystem.Keyboard keyboard &&
                   (keyboard.downArrowKey.wasPressedThisFrame ||
                    keyboard.upArrowKey.wasPressedThisFrame ||
                    keyboard.leftArrowKey.wasPressedThisFrame ||
                    keyboard.rightArrowKey.wasPressedThisFrame);
            #else
            return false;
            #endif
        }

        protected virtual bool SampleLegacyInputNavigation ()
        {
            #if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.UpArrow) ||
                   Input.GetKeyDown(KeyCode.DownArrow) ||
                   Input.GetKeyDown(KeyCode.LeftArrow) ||
                   Input.GetKeyDown(KeyCode.RightArrow);
            #else
            return false;
            #endif
        }

        private RectTransform GetRectTransform ()
        {
            if (!rectTransform)
                rectTransform = GetComponent<RectTransform>();
            return rectTransform;
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
