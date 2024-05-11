using System.Collections;
using UnityEngine;
using Yu5h1Lib;

public abstract class UI_DialogBase : UI_Behaviour
{
    
    public string[] lines;
    public abstract string Content { get; set; }
    public float speed = 0.05f;
    public int StepIndex { get; private set; }
    public bool IsPerforming { get; private set; }
    public bool NothingToSay => StepIndex >= lines.Length - 1 && !IsPerforming;
    private void OnEnable()
    {
        if (lines.IsEmpty())
            return;
        StartCoroutine(PerformVerbatim(lines[StepIndex = 0]));
    }
    private void OnDisable()
    {
    
    }
    IEnumerator PerformVerbatim(string text)
    {
        if (speed == 0)
        {
            Content = text;            
            yield break;
        }
        IsPerforming = true;
        Content = "";
        foreach (var letter in text.ToCharArray())
        {
            if (!IsPerforming)
            {
                Content = text;
                yield break; 
            }
            Content += letter;
            yield return new WaitForSecondsRealtime(speed);
        }
        IsPerforming = false;
    }
    #region Action
    public bool Next()
    {
        if (NothingToSay)
            return false;
        StartCoroutine(PerformVerbatim(lines[++StepIndex]));
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
            if (GameManager.IsGamePause)
                GameManager.IsGamePause = false;
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
