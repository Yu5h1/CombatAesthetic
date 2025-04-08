using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Yu5h1Lib;
using Yu5h1Lib.Game.Character;

public class GameManagementAgent : MonoBehaviour,IGameManager
{
    public  GameManager manager
    { 
        get {
            if (GameManager.IsQuit)
                return null;
            return GameManager.instance;
        }
    }

    public void Continue()
    {
        if (GameManager.IsGamePaused)
            GameManager.IsGamePaused = false;
    }
    public void SetGamePause(bool pasue) => GameManager.IsGamePaused = pasue;

    public void ChangeBGMvolume(float val) => AudioManager.bgmVolume = val;
    public void ChangeSFXvolume(float val) => AudioManager.sfxVolume = val;
    public void LoadBGMvolume(Slider s) => s.value = AudioManager.bgmVolume;
    public void LoadSFXvolume(Slider s) => s.value = AudioManager.sfxVolume;
    public void ToggleUseWorldInputAxis(bool toggle) => PlayerHost.UseWorldInputAxis = toggle;
    public void LoadUseWorldInputAxis(Toggle toggle) => toggle.isOn = PlayerHost.UseWorldInputAxis;
    public void SetLoadingStory(string name)
    {
        if (GameManager.IsQuit)
            return;
        GameManager.storyManager.loadingStory = name;
    }

    #region Scene
    public void DelayLoadStartScene(float delay)
        => GameManager.instance.StartCoroutine(LoadSceneProcess(delay, 0));
    IEnumerator LoadSceneProcess(float delay, int index)
    {
        if (delay > 0)
            yield return new WaitForSecondsRealtime(delay);
        SceneController.LoadScene(index);
    }

    public void EnablePlayerControl()
    {
        if (GameManager.IsQuit)
            return;
        manager.EnablePlayerControl();
    }

    public void DisablePlayerControl()
    {
        if (GameManager.IsQuit)
            return;
        manager.DisablePlayerControl();
    }
    public void SetUIVisible(bool visible)
    {
        if (GameManager.IsQuit)
            return;
        UI_Manager.visible = visible;
    }
    #endregion
}
