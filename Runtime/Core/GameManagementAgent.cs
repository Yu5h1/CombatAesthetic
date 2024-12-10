using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Yu5h1Lib;

public class GameManagementAgent : MonoBehaviour
{
    public static GameManager GameManager => GameManager.instance;

    Coroutine LoadStartSceneCoroutine;

    public void ContinueTheGameIfPause()
    {
        if (GameManager.IsGamePause)
            GameManager.IsGamePause = false;
    }
    public void ChangeBGMvolume(float val) => SoundManager.bgmVolume = val;
    public void ChangeSFXvolume(float val) => SoundManager.sfxVolume = val;

    public void DelayLoadStartScene(float delay)
    {
        this.StartCoroutine(ref LoadStartSceneCoroutine, LoadSceneProcess(delay, 0));
    }
    IEnumerator LoadSceneProcess(float delay,int index)
    { 
        if (delay > 0)
            yield return new WaitForSecondsRealtime(delay);
        SceneController.LoadScene(index);
    }
}
