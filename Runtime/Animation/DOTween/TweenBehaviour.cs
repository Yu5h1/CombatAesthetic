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
    public bool RewindOnDisable = true;
    public bool UseUnscaledTime = false;
    public bool IsWaiting { get; protected set; }

    public Tweener tweener { get ; protected set; }
    protected abstract Tweener Create();
    public bool IsInitinalized => tweener != null;
    protected abstract void Init();
    public void Kill()
    {
        if (!IsInitinalized)
            return;
        tweener.Kill();
        tweener = null;
    }
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
    public event UnityAction<TweenBehaviour, TValue> CompleteEvent;

    [SerializeField]
    protected UnityEvent<TValue> OnRewindEvent;
    public event UnityAction<TweenBehaviour, TValue> RewindEvent;

    protected override Tweener Create() => CreateTweenCore();
    protected abstract TweenerCore<TValue, TValue, TPlugOptions> CreateTweenCore();


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
        Init();
        if (!playOnEnable)
            return;
        PlayTween();
    }
    protected void OnRewind(TValue value)
    {
        OnRewindEvent?.Invoke(value);
        RewindEvent?.Invoke(this, value);
    }

    protected virtual void OnRewind()
        => OnRewind(TweenerCore.changeValue);
    
    protected virtual void OnLoop()
    {

    }
    protected void OnComplete(TValue value)
    {
        OnCompleteEvent?.Invoke(value);
        CompleteEvent?.Invoke(this, TweenerCore.changeValue);
    }
    protected virtual void OnComplete()
        => OnComplete(TweenerCore.changeValue);

    private void OnDisable()
    {
        if (RewindOnDisable)
            tweener.Rewind();
        else
            tweener.Pause();

        //Kill();

        //if (!playOnEnable && TweenerCore?.IsComplete() == true)
        //{
        //    tweener.Pause();
        //    tweener.Rewind();
        //}
    }
    protected void PlayTween()
    {
        //tweener.ForceInit();
        //tweener.Rewind();
        tweener.PlayForward();
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