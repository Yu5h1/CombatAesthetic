using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using System.Collections;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;

public abstract class TweenBehaviour : MonoBehaviour
{
    public float Delay;
    public float Duration = 1;
    public int LoopCount = 0;
    public LoopType LoopType = LoopType.Yoyo;
    public Ease EaseType;
    public bool playOnEnable = true;
    public bool UseUnscaledTime = false;
    public bool IsWaiting { get; protected set; }
    protected Tweener tweener;
    protected abstract Tweener Create();
    protected abstract void Init();
}
public abstract class TweenBehaviour<TComponent, TValue, TPlugOptions> : TweenBehaviour
    where TComponent : Component
    where TPlugOptions : struct, IPlugOptions
{
    public TComponent component { get; private set; }
    protected TweenerCore<TValue, TValue, TPlugOptions> TweenerCore 
    { get => (TweenerCore<TValue, TValue, TPlugOptions>)tweener; set => tweener = value; }

    [SerializeField]
    protected TValue _startValue;
    public TValue startValue { get => _startValue; protected set => _startValue = TweenerCore.endValue = value; }
    [SerializeField]
    private bool ChangeStartValue;
    [SerializeField]
    protected TValue _endValue;
    public TValue endValue { get => _endValue; protected set => _endValue = TweenerCore.endValue = value; }

    [SerializeField]
    protected UnityEvent<TValue> OnCompleteEvent;
    [SerializeField]
    protected UnityEvent<TValue> OnRewindEvent;

    protected override Tweener Create() => CreateTweenCore();
    protected abstract TweenerCore<TValue, TValue, TPlugOptions> CreateTweenCore();

    public bool IsInitinalized => TweenerCore != null;
    public void Kill()
    {
        if (!IsInitinalized)
            return;
        TweenerCore.Kill();
        TweenerCore = null;
    }

    protected override void Init()
    {
        if (IsInitinalized)
            return;
        
        component = GetComponent<TComponent>();
        TweenerCore = CreateTweenCore();

        if (ChangeStartValue)
            TweenerCore.ChangeStartValue(startValue);
        if (UseUnscaledTime)
            TweenerCore.SetUpdate(UseUnscaledTime);
        if (LoopCount != 0)
            TweenerCore.SetLoops(LoopCount, LoopType);
        if (!playOnEnable)
            TweenerCore.Pause();
        if (Delay > 0)
            TweenerCore.SetDelay(Delay);
        TweenerCore.onComplete += OnComplete;
        TweenerCore.onStepComplete += OnLoop;
        TweenerCore.onRewind += OnRewind;
        TweenerCore.SetAutoKill(false);
        if (EaseType != Ease.Unset)
            TweenerCore.SetEase(EaseType);
    }

    protected virtual void Start()
    {
        Init();
    }
    private void OnEnable()
    {        
        if (!playOnEnable)
            return;
        if (!IsInitinalized)
            Init();
        PlayTween();
    }
    protected virtual void OnRewind()
    {
        OnRewindEvent?.Invoke(TweenerCore.changeValue);
    }
    protected virtual void OnLoop()
    {

    }
    protected virtual void OnComplete()
    {
        OnCompleteEvent?.Invoke(TweenerCore.changeValue);
    }
    private void OnDisable()
    {
        if (!playOnEnable && TweenerCore?.IsComplete() == true)
        {
            TweenerCore.Pause();
            TweenerCore.ForceInit();
            TweenerCore.Rewind();
        }
    }
    protected void PlayTween()
    {
        TweenerCore.ForceInit();
        TweenerCore.Rewind();
        TweenerCore.PlayForward();
    }
    public void TryPlayTween()
    {
        if (TweenerCore.IsPlaying() || IsWaiting)
            return;
        PlayTween();
    }
    Coroutine coroutine;
    public void TryPlayBackwards(float delay)
    {
        if (TweenerCore.IsPlaying() || IsWaiting)
            return;
        if (coroutine != null)
            StopCoroutine(coroutine);
        coroutine = StartCoroutine(EnumPlayBackwards(delay));
    }
    IEnumerator EnumPlayBackwards(float delay)
    {
        IsWaiting = true;
        yield return new WaitForSeconds(delay);
        TweenerCore.PlayBackwards();
        IsWaiting = false;
    }
    private void OnDestroy()
    {
        TweenerCore.Kill();
    }
    
}