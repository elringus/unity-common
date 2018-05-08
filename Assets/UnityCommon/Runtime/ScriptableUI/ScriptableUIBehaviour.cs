using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class ScriptableUIBehaviour : UIBehaviour
{
    public event Action<bool> OnVisibilityChanged;

    public float FadeTime { get { return _fadeTime; } set { _fadeTime = value; } }
    public bool IsVisibleOnAwake { get { return _isVisibleOnAwake; } }
    public virtual bool IsVisible { get { return _isVisible; } set { SetIsVisible(value); } }
    public virtual float CurrentOpacity { get { return GetCurrentOpacity(); } }
    public virtual bool IsInteractable => canvasGroup ? canvasGroup.interactable : true;
    public RectTransform RectTransform { get { return GetRectTransform(); } }

    private Tweener<FloatTween> fadeTweener;
    private CanvasGroup canvasGroup;
    private RectTransform _rectTransform;
    private bool _isVisible;

    [Tooltip("Whether UI element should be visible or hidden on awake.")]
    [SerializeField] private bool _isVisibleOnAwake = true;
    [Tooltip("Fade duration (in seconds) when changing visiblity.")]
    [SerializeField] private float _fadeTime = .3f;

    protected override void Awake ()
    {
        base.Awake();

        fadeTweener = new Tweener<FloatTween>(this);
        canvasGroup = GetComponent<CanvasGroup>();
        SetIsVisible(IsVisibleOnAwake);
    }

    public Canvas GetTopmostCanvas ()
    {
        var parentCanvases = gameObject.GetComponentsInParent<Canvas>();
        if (parentCanvases != null && parentCanvases.Length > 0)
            return parentCanvases[parentCanvases.Length - 1];
        return null;
    }

    public virtual async Task SetIsVisibleAsync (bool isVisible, float? fadeTime = null)
    {
        if (fadeTweener.IsRunning)
            fadeTweener.Stop();

        _isVisible = isVisible;

        OnVisibilityChanged.SafeInvoke(isVisible);

        if (!canvasGroup) return;

        canvasGroup.interactable = isVisible;
        canvasGroup.blocksRaycasts = isVisible;

        var fadeDuration = fadeTime ?? FadeTime;
        var targetOpacity = isVisible ? 1f : 0f;

        if (fadeDuration == 0f)
        {
            canvasGroup.alpha = targetOpacity;
            return;
        }

        var tween = new FloatTween(canvasGroup.alpha, targetOpacity, fadeDuration, alpha => canvasGroup.alpha = alpha);
        await fadeTweener.RunAsync(tween);
    }

    public virtual void SetIsVisible (bool isVisible)
    {
        if (fadeTweener.IsRunning)
            fadeTweener.Stop();

        _isVisible = isVisible;

        OnVisibilityChanged.SafeInvoke(isVisible);

        if (!canvasGroup) return;

        canvasGroup.interactable = isVisible;
        canvasGroup.blocksRaycasts = isVisible;

        canvasGroup.alpha = isVisible ? 1f : 0f;
    }

    public virtual void ToggleVisibility ()
    {
        SetIsVisibleAsync(!IsVisible).WrapAsync();
    }

    public virtual void Show ()
    {
        SetIsVisibleAsync(true).WrapAsync();
    }

    public virtual void Hide ()
    {
        SetIsVisibleAsync(false).WrapAsync();
    }

    public virtual float GetCurrentOpacity ()
    {
        if (canvasGroup) return canvasGroup.alpha;
        return 1f;
    }

    public virtual void SetOpacity (float opacity)
    {
        if (!canvasGroup) return;

        canvasGroup.alpha = opacity;
    }

    public virtual void SetInteractable (bool isInteractable)
    {
        if (!canvasGroup) return;

        canvasGroup.interactable = isInteractable;
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

    private RectTransform GetRectTransform ()
    {
        if (!_rectTransform)
            _rectTransform = GetComponent<RectTransform>();
        return _rectTransform;
    }
}
