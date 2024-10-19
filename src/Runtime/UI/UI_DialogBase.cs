using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Yu5h1Lib;

public abstract class UI_DialogBase : UI_Behaviour
{
    
    public string[] lines;
    public abstract string Content { get; set; }
    public float speed = 0.05f;
    public int StepIndex { get; private set; }
    public bool IsPerforming { get; private set; }
    public bool NothingToSay => StepIndex >= lines.Length - 1 && !IsPerforming;

    [SerializeField]
    private UnityEvent OnSkip;
    [SerializeField]
    private UnityEvent OnPerformCompleted;
    [SerializeField]
    private UnityEvent OnBeginShowWord;

    private Timer timer = new Timer();
    private Timer.Wait<Timer> wait;

    public event UnityAction PerformCompleted {
        add => OnPerformCompleted.AddListener(value);
        remove => OnPerformCompleted.RemoveListener(value);
    }

    public event UnityAction BeginShowWord
    {
        add => OnBeginShowWord.AddListener(value);
        remove => OnBeginShowWord.RemoveListener(value);
    }

    private IEnumerator lastCoroutine;

    private void Start()
    {
        timer.duration = speed;
        wait = timer.Waiting();
    }

    private void OnEnable()
    {
        if (lines.IsEmpty())
            return;
        Perform();
    }
    private void OnDisable()
    {
    
    }
    private void Perform()
    {
        if (lastCoroutine != null)
            StopCoroutine(lastCoroutine);
        StartCoroutine(lastCoroutine = PerformVerbatimProcess(lines[StepIndex = 0]));
    }
    public void PerformVerbatim(string content)
    {
        lines = new string[] { content };
        Perform();
    }

    IEnumerator PerformVerbatimProcess(string text)
    {
        if (speed == 0)
        {
            Content = text;            
            yield break;
        }
        IsPerforming = true;
        Content = "";

        for (int i = 0; i < text.Length; i++)
        {
            var letter = text[i];
            if (!IsPerforming)
            {
                Content = text;
                OnPerformCompleted?.Invoke();
                yield break;
            }
            Content += letter;
            OnBeginShowWord?.Invoke();
            yield return new WaitForSeconds(speed);
        }

        OnPerformCompleted?.Invoke();
        IsPerforming = false;
    }
    #region Action
    public bool Next()
    {
        if (NothingToSay)
            return false;
        StartCoroutine(PerformVerbatimProcess(lines[++StepIndex]));
        return true;
    }

    public void Skip()
    {
        if (!gameObject.activeSelf)
            return;
        if (IsPerforming)
            IsPerforming = false;
        else if (!Next())
        {
            gameObject.SetActive(false);
            OnSkip?.Invoke();
        }
    } 
    #endregion
    public void AddElementFromContent()
    {
        var newIndex = lines.Length;
        System.Array.Resize(ref lines, newIndex + 1);
        lines[newIndex] = Content;
    }
}
