using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Yu5h1Lib;

public class GameManagementAgent : MonoBehaviour
{
    public static GameManager GameManager => GameManager.instance;

    public void ContinueTheGameIfPause()
    {
        if (GameManager.IsGamePause)
            GameManager.IsGamePause = false;
    }
    public void ChangeBGMvolume(float val) => SoundManager.bgmVolume = val;
    public void ChangeSFXvolume(float val) => SoundManager.sfxVolume = val;

}
