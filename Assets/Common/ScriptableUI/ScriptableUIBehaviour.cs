using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class OnUIVisibilityChanged : UnityEvent<bool> { }

public abstract class ScriptableUIBehaviour : UIBehaviour
{
    public float FadeTime = .3f;

    public readonly UnityEvent OnFadeComplete = new UnityEvent();
    public readonly OnUIVisibilityChanged OnVisibilityChanged = new OnUIVisibilityChanged();

    public RectTransform RectTransform
    {
        get
        {
            if (!_rectTransform)
                _rectTransform = GetComponent<RectTransform>();
            return _rectTransform;
        }
    }

    public bool IsVisibleOnAwake
    {
        get { return _isVisibleOnAwake; }
        set { _isVisibleOnAwake = value; }
    }

    public virtual bool IsVisible
    {
        get { return _isVisible; }
        set { SetIsVisible(value); }
    }

    public virtual float CurrentOpacity { get { return GetCurrentOpacity(); } }

    [SerializeField] private bool _isVisibleOnAwake = true;
    private RectTransform _rectTransform;
    private CanvasGroup canvasGroup;
    private bool _isVisible;

    protected override void Awake ()
    {
        base.Awake();

        canvasGroup = GetComponent<CanvasGroup>();
        SetIsVisible(IsVisibleOnAwake, 0f);
    }

    public virtual void SetIsVisible (bool isVisible, float? fadeTime = null)
    {
        _isVisible = isVisible;

        OnVisibilityChanged.Invoke(isVisible);

        if (!canvasGroup) { OnFadeComplete.Invoke(); return; }

        canvasGroup.interactable = isVisible;
        canvasGroup.blocksRaycasts = isVisible;

        var fadeDuration = fadeTime ?? FadeTime;
        var targetOpacity = isVisible ? 1f : 0f;

        if (fadeDuration == 0f)
        {
            canvasGroup.alpha = targetOpacity;
            OnFadeComplete.Invoke();
            return;
        }

        var tween = new FloatTween(canvasGroup.alpha, targetOpacity, fadeDuration, alpha => canvasGroup.alpha = alpha);
        new Tweener<FloatTween>(this, () => OnFadeComplete.Invoke()).Run(tween);
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
}

