using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yu5h1Lib;

public class GameEnding : MonoBehaviour
{
	public void Perform()
	{
        var t = DOVirtual.Float(1, 0, 0.2f, FadeTime);
        t.SetUpdate(true);
        //t.ForceInit();
        //t.Rewind();
        //t.PlayForward();

        PoolManager.canvas.sortingLayerName = "Back";
        CameraController.instance.FadeIn("Back", 1);
        GameManager.ui_Manager.LevelSceneMenu.FadeIn(1f);
        GameManager.instance.Player.attribute.ui.gameObject.SetActive(false);
    }
    public void FadeTime(float f)
    {
        Time.timeScale = f;
    }
}
