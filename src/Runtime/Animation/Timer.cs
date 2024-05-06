using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class Timer : ITimer
{
    [SerializeField]
    private float Delay = 0;
    [SerializeField]
    private float Duration = 1;
    [SerializeField]
    private int LoopCount;

    public float delay { get => Delay; set => Delay = value; }
    public float duration { get => Duration; set => Duration = value; }
    public int loopCount { get => LoopCount; set => LoopCount = value; }

    [SerializeField]
    private UnityEvent _Update;
    [SerializeField]
    private UnityEvent _Completed;
    [SerializeField]
    private UnityEvent _Looped;
    [SerializeField]
    private UnityEvent _FinalLooped;
    public event UnityAction Update
    {
        add {
            if (_Update == null)
                _Update = new UnityEvent();
            _Update.AddListener(value);
        }
        remove {
            if (_Update == null)
                return;
            _Update.RemoveListener(value);
        } 
    }
    public event UnityAction Completed
    {
        add
        {
            if (_Completed == null)
                _Completed = new UnityEvent();
            _Completed.AddListener(value);
        }
        remove
        {
            if (_Completed == null)
                return;
            _Completed.RemoveListener(value);
        }
    }
    public event UnityAction Looped
    {
        add
        {
            if (_Looped == null)
                _Looped = new UnityEvent();
            _Looped.AddListener(value);
        }
        remove
        {
            if (_Looped == null)
                return;
            _Looped.RemoveListener(value);
        }
    }
    public event UnityAction FinalLooped
    {
        add
        {
            if (_FinalLooped == null)
                _FinalLooped = new UnityEvent();
            _FinalLooped.AddListener(value);
        }
        remove
        {
            if (_FinalLooped == null)
                return;
            _FinalLooped.RemoveListener(value);
        }
    }

    #region Caches
    protected float LastTime { get; private set; }
    protected int loopCounter { get; private set; }
    public bool IsCompleted { get; private set; }
    public bool CheckIsCompleted() => IsCompleted;
    #endregion

    protected virtual float GetTime() => Time.time;
    public float time => GetTime() - LastTime;
    public bool IsStart => time > 0;
    public bool TimesUp => time >= duration;
    public bool IsCompleting => loopCounter == 0 && TimesUp;
    public float normal => time.GetNormal(duration);

    public virtual void Start()
    {        
        LastTime = GetTime() + delay;
        loopCounter = loopCount;
        IsCompleted = false;
        
    }
    public virtual void Stop()
        => LastTime = time - duration;

    public virtual void Tick()
    {
        if (IsCompleted)
            return;
        if (IsCompleting)
        {
            OnCompleted();
            _Completed?.Invoke();
            IsCompleted = true;
        }
        else if (TimesUp)
        {
            if (loopCounter > 0)
            {
                if (loopCounter == 1)
                {
                    OnFinalLoop();
                    _FinalLooped?.Invoke();
                }
                loopCounter--;
            }
            OnLoop();
            _Looped?.Invoke();
            LastTime = time;
        }
        else
            _Update?.Invoke();
    }
    protected virtual void OnLoop() {}
    protected virtual void OnFinalLoop(){}
    protected virtual void OnCompleted(){}
    public void EndLoop() => loopCounter = 0;

    public override string ToString() => $"Duration:{Duration} IsCompleted:{IsCompleted}";

    public class Wait<T> : CustomYieldInstruction where T : Timer
    {
        public T timer { get; set; }
        public override bool keepWaiting
        {
            get
            {
                timer.Tick();
                if (timer.IsCompleted)
                { 
                    timer = null;
                    return false;
                }
                return true;
            }
        }
        public Wait(T t) {
            timer = t;
            timer.Start();
        }  
    }

}
public interface ITimer
{
    float delay { get; set; }
    float duration { get; set; }
    int loopCount { get; set; }
}