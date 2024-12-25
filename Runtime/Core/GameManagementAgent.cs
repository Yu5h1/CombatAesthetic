using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yu5h1Lib;
using Yu5h1Lib.Game.Character;

public class GameManagementAgent : MonoBehaviour
{
    public static GameManager GameManager => GameManager.instance;

    Coroutine LoadStartSceneCoroutine;

    public void Continue()
    {
        if (GameManager.IsGamePause)
            GameManager.IsGamePause = false;
    }
    public void ChangeBGMvolume(float val) => SoundManager.bgmVolume = val;
    public void ChangeSFXvolume(float val) => SoundManager.sfxVolume = val;
    public void LoadBGMvolume(Slider s) => s.value = SoundManager.bgmVolume;
    public void LoadSFXvolume(Slider s) => s.value = SoundManager.sfxVolume;
    public void ToggleUseWorldInputAxis(bool toggle) => PlayerHost.UseWorldInputAxis = toggle;
    public void LoadUseWorldInputAxis(Toggle toggle) => toggle.isOn = PlayerHost.UseWorldInputAxis;

    #region Scene
    public void DelayLoadStartScene(float delay)
    {
        this.StartCoroutine(ref LoadStartSceneCoroutine, LoadSceneProcess(delay, 0));
    }
    IEnumerator LoadSceneProcess(float delay, int index)
    {
        if (delay > 0)
            yield return new WaitForSecondsRealtime(delay);
        SceneController.LoadScene(index);
    }
    #endregion
}
