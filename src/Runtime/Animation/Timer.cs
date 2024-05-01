using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public abstract class Timer : ITimer
{
    [SerializeField]
    private float Delay = 0;
    [SerializeField]
    private float Duration = 1;
    [SerializeField]
    private int LoopCount;

    public float delay => Delay;
    public float duration => Duration;
    public int loopCount => LoopCount;

    public System.Action Completed;
    public System.Action Looped;
    public System.Action FinalLooped;

    #region Caches
    protected float LastTime;
    protected int loopCounter = 0;
    private bool IsCompleted = false;
    #endregion

    protected virtual float GetTime() => Time.time;
    public float time => GetTime() - LastTime;
    public bool IsStart => time > 0;
    public bool TimesUp => time >= duration;
    public bool IsCompleting => loopCounter == 0 && TimesUp;
    public float normal => time.GetNormal(duration);

    public virtual void Start()
    {
        LastTime = time + delay;
        loopCounter = loopCount;
        IsCompleted = false;
    }
    public virtual void Stop()
    {
        LastTime = time - duration;
    }
    protected virtual void Update()
    {
        if (IsCompleted)
            return;
        if (IsCompleting)
        {
            OnCompleted();
            Completed?.Invoke();
            IsCompleted = true;
        }
        else if (TimesUp)
        {
            if (loopCounter > 0)
            {
                if (loopCounter == 1)
                {
                    OnFinalLoop();
                    FinalLooped?.Invoke();
                }
                loopCounter--;
            }
            OnLoop();
            Looped?.Invoke();
            LastTime = time;
        }
    }
    protected virtual void OnLoop() {}
    protected virtual void OnFinalLoop(){}
    protected virtual void OnCompleted(){}
    public void EndLoop() => loopCounter = 0;
}
public interface ITimer
{
    float delay { get; }
    float duration { get; }
    int loopCount { get; }
}