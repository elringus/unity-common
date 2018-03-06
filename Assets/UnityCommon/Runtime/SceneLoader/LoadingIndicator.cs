using UnityEngine;

public class LoadingIndicator : ScriptableUIBehaviour
{
    [SerializeField] private RectTransform loadingIcon = default(RectTransform);
    [SerializeField] private float animationTime = 1f;
    [SerializeField] private float animationDelay = 2f;

    private Timer timer;
    private Tweener<FloatTween> tweener;
    private FloatTween tween;

    protected override void Awake ()
    {
        base.Awake();

        this.AssertRequiredObjects(loadingIcon);

        timer = new Timer(animationDelay, false, true, this, () => tweener.Run());
        tween = new FloatTween(0, -180, animationTime, value => loadingIcon.rotation = Quaternion.Euler(0, 0, value), true, true);
        tweener = new Tweener<FloatTween>(tween, this, () => timer.Run());
    }

    protected override void Start ()
    {
        base.Start();

        tweener.Run();

        var sceneLoader = Context.Resolve<SceneLoader>(assertResult: true);
        if (sceneLoader.IsReadyToActivate) SetIsVisible(false, 0f);
        else sceneLoader.OnReadyToActivate += () => SetIsVisible(false);
    }
}
