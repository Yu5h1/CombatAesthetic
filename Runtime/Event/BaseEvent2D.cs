using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yu5h1Lib;

public class BaseEvent2D : MonoBehaviourEnhance
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
    public void Prompt(string line)
    {
        GameManager.ui_Manager.Dialog_UI.lines = new string[] { line };
        GameManager.ui_Manager.Dialog_UI.gameObject.SetActive(true);
    }
    public void Prompt(string[] lines)
    {
        GameManager.ui_Manager.Dialog_UI.lines = lines;
        GameManager.ui_Manager.Dialog_UI.gameObject.SetActive(true);
    }
}
