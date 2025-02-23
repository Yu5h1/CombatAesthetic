using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yu5h1Lib;

public class BaseEvent2D : MonoBehaviour
{
    public void PlayAudio()
    {
        if (TryGetComponent(out AudioSource audioSource))
            GameManager.instance.PlayAudio(audioSource);
    }
    public void Spawn(string name)
    {
        PoolManager.Spawn<Transform>(name, transform.position, transform.rotation);
    }
    public void Prompt(string[] lines) => GameManager.ui_Manager.Prompt(lines);
    public void Prompt(string content) => GameManager.ui_Manager.Prompt(content);

    #region GameManager

    public void SetPlayerControllable(bool flag) => GameManager.SetPlayerControllable(flag);

    #endregion


    #region animated
    Coroutine SetActiveCoroutine;
    public void Deactivate(float delay)
        => this.StartCoroutine(ref SetActiveCoroutine, DelaySetActive(delay, false));
    public void Activate(float delay)
        => this.StartCoroutine(ref SetActiveCoroutine, DelaySetActive(delay, true));
    private IEnumerator DelaySetActive(float delay, bool active)
    {
        if (delay > 0)
            yield return new WaitForSeconds(delay);
        gameObject.SetActive(active);
    } 
    #endregion
    public void Log(string msg) => msg.print();
}
