using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yu5h1Lib;

public class StoryPerformance : MonoBehaviour
{
    public static StoryPerformance current { get; set; }
    [SerializeField,ReadOnly]
    private bool _completed;
    public bool IsCompleted => _completed;

    public void Play()
    {
        current = this;

    }
    public void Stop()
    {
        _completed = true;
    }


    public IEnumerator WaitCompleted()
    {
        yield return new WaitUntil(CheckCompleted);
        current = null;
    }
    private bool CheckCompleted() => _completed;

    public void MarkAsCompleted() => _completed = true;
    public void MarkAsIncomplete() => _completed = false;
}
