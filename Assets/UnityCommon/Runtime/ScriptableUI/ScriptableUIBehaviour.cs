using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ScriptableUIBehaviour : UIBehaviour
{
    public event UnityAction OnFadeComplete;
    public event UnityAction<bool> OnVisibilityChanged;

    public float FadeTime { get { return _fadeTime; } set { _fadeTime = value; } }
    public bool IsVisibleOnAwake { get { return _isVisibleOnAwake; } }
    public virtual bool IsVisible { get { return _isVisible; } set { SetIsVisible(value); } }
    public virtual float CurrentOpacity { get { return GetCurrentOpacity(); } }
    public RectTransform RectTransform { get { return GetRectTransform(); } }

    private RectTransform _rectTransform;
    private CanvasGroup canvasGroup;
    private bool _isVisible;

    [Tooltip("Whether UI element should be visible or hidden on awake.")]
    [SerializeField] private bool _isVisibleOnAwake = true;
    [Tooltip("Fade duration (in seconds) when changing visiblity.")]
    [SerializeField] private float _fadeTime = .3f;

    protected override void Awake ()
    {
        base.Awake();

        canvasGroup = GetComponent<CanvasGroup>();
        SetIsVisible(IsVisibleOnAwake, 0f);
    }

    public virtual void SetIsVisible (bool isVisible, float? fadeTime = null)
    {
        _isVisible = isVisible;

        OnVisibilityChanged.SafeInvoke(isVisible);

        if (!canvasGroup) { OnFadeComplete.SafeInvoke(); return; }

        canvasGroup.interactable = isVisible;
        canvasGroup.blocksRaycasts = isVisible;

        var fadeDuration = fadeTime ?? FadeTime;
        var targetOpacity = isVisible ? 1f : 0f;

        if (fadeDuration == 0f)
        {
            canvasGroup.alpha = targetOpacity;
            OnFadeComplete.SafeInvoke();
            return;
        }

        var tween = new FloatTween(canvasGroup.alpha, targetOpacity, fadeDuration, alpha => canvasGroup.alpha = alpha);
        new Tweener<FloatTween>(this, () => OnFadeComplete.SafeInvoke()).Run(tween);
    }

    public virtual void ToggleVisibility (float? fadeTime = null)
    {
        SetIsVisible(!IsVisible, fadeTime);
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
