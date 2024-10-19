using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yu5h1Lib;

public class GameEnding : MonoBehaviour
{
    private static void EndCreditsPerformComplete(TweenBehaviour s, Vector3 val)
    {
        s.gameObject.SetActive(false);
        SceneController.LoadScene(0);
    }

    public void Perform(float delay)
	{
        
        #region fade TimeScale
        var t = DOVirtual.Float(1, 0, 0.2f, FadeTime);
        t.SetUpdate(true); 
        #endregion
        PoolManager.canvas.sortingLayerName = "Back";
        CameraController.instance.FadeIn("Back",Color.white, 1);
        GameManager.ui_Manager.EndCredits.gameObject.SetActive(true);

        var domove2D = GameManager.ui_Manager.EndCredits.GetComponent<DOMove2D>();
        domove2D.CompleteEvent -= EndCreditsPerformComplete;
        domove2D.CompleteEvent += EndCreditsPerformComplete;

        GameManager.instance.playerController.attribute.ui.gameObject.SetActive(false);
    }


    public void FadeTime(float f)
    {
        Time.timeScale = f;
    }
}
