using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public abstract class Timer : MonoBehaviour , ITimer
{
    [System.Serializable]
    public class Info : ITimer
    {
        [SerializeField]
        private Timer RefTimer;
        [SerializeField]
        private float Delay = 0;
        [SerializeField]
        private float Duration = 1;
        [SerializeField]
        private bool UseUnscaledTime = false;
        [SerializeField]
        private int LoopCount;
        [SerializeField]
        private bool DestoryOnComplete;

        public Timer refTimer { get => RefTimer; set => RefTimer = value; }
        public float delay { get => Delay; set => Delay = value; }
        public float duration { get => Duration; set => Duration = value; }
        public bool useUnscaledTime { get => UseUnscaledTime; set => UseUnscaledTime = value; }
        public int loopCount { get => LoopCount; set => LoopCount = value; }
        public bool destoryOnComplete { get => DestoryOnComplete; set => DestoryOnComplete = value; }
    }
    public Info data;
    public Timer refTimer { get => data.refTimer; set => data.refTimer = value; }
    public float delay { get => data.delay; set => data.delay = value; }
    public float duration { get => data.duration; set => data.duration = value; }
    public bool useUnscaledTime { get => data.useUnscaledTime; set => data.useUnscaledTime = value; }
    public int loopCount { get => data.loopCount; set => data.loopCount = value; }
    public bool destoryOnComplete { get => data.destoryOnComplete; set => data.destoryOnComplete = value; }

    public UnityEvent OnCompleteEvents;

    #region Caches
    protected float LastTime;
    protected int loopCounter = 0;
    private bool IsAreadyComplete = false;
    #endregion

    public static float GetTime(bool UseUnscaledTime = false) => UseUnscaledTime ? Time.unscaledTime : Time.time;
    public float time => refTimer == null ? Timer.GetTime(data.useUnscaledTime) - LastTime : refTimer.time;
    public bool IsStart => refTimer == null ? refTimer.IsStart : time > 0;
    public bool TimesUp => refTimer == null ? refTimer.TimesUp : time >= duration;
    public virtual bool IsComplete => refTimer == null ? refTimer.IsComplete : (loopCounter == 0 && TimesUp);
    public float normal => refTimer == null ? refTimer.normal : time.GetNormal(duration);

    protected virtual void Launch()
    {
        if (refTimer != null)
            return;
        LastTime = Timer.GetTime(data.useUnscaledTime) + delay;
        loopCounter = loopCount;
        IsAreadyComplete = false;
    }
    public void Stop()
    {
        if (refTimer == null)
            LastTime = Timer.GetTime(data.useUnscaledTime) - duration;
        else
            refTimer.Stop();
    }
    protected virtual void OnEnable() =>  Launch();

    protected virtual void Update()
    {
        if (refTimer != null || IsAreadyComplete)
            return;
        if (IsComplete)
        {
            OnComplete();
            IsAreadyComplete = true;
            if (destoryOnComplete)
                DestroySelf();
        }
        else if (TimesUp)
        {
            if (loopCounter > 0)
            {
                if (loopCounter == 1)
                    OnFinalLoop();
                loopCounter--;
            }
            OnLoop();
            LastTime = Timer.GetTime(data.useUnscaledTime);
        }
    }
    protected virtual void OnLoop() { }
    protected virtual void OnFinalLoop()
    {
        Debug.LogWarning("OnFinalLoop");
    }
    protected virtual void OnComplete()
    {
        OnCompleteEvents.Invoke();
    }

    public virtual void EndLoop()
    {
        Debug.LogWarning(gameObject.name + " EndLoop");
        loopCounter = 0;
    }
    public virtual void DestroySelf()
    {
        GameObject.Destroy(this);
    }

}
public interface ITimer
{
    Timer refTimer { get; set; }
    float delay { get; set; }
    float duration { get; set; }
    bool useUnscaledTime { get; set; }
    int loopCount { get; set; }
    bool destoryOnComplete { get; set; }
}