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
        PoolManager.instance.Spawn<Transform>(name, transform.position, transform.rotation);
    }
    public void Prompt(string[] lines)
    {
        GameManager.ui_Manager.Dialog_UI.lines = lines;
        GameManager.ui_Manager.Dialog_UI.transform.SetAsLastSibling();
        GameManager.ui_Manager.Dialog_UI.gameObject.SetActive(true);
    }
    public void Prompt(string line) => Prompt(new string[] { line });
    Coroutine SetActiveCoroutine;
    public void Deactivate(float delay)
        => this.StartCoroutine(ref SetActiveCoroutine, DelaySetActive(delay, false));
    public void Activate(float delay)
        => this.StartCoroutine(ref SetActiveCoroutine, DelaySetActive(delay, true));
    private IEnumerator DelaySetActive(float delay,bool active)
    { 
        if (delay > 0)
            yield return new WaitForSeconds(delay);
        gameObject.SetActive(active);

    }

}
