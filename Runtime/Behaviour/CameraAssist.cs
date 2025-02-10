using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yu5h1Lib;

public class CameraAssist : MonoBehaviour
{
    CameraController controller => CameraController.instance;
    [TextArea(1,10)]
    public string content;
    public Transform[] targets;
    public float duration = 2;

    Coroutine coroutine;

    public void Perform()
    {
        this.StartCoroutine(ref coroutine, PerformFocusProcess());
    }

    bool ConversationOver() => !GameManager.ui_Manager.Dialog_UI.IsPerforming && !GameManager.ui_Manager.Dialog_UI.isActiveAndEnabled;
    //public void Prompt(string[] lines) => GameManager.ui_Manager.Prompt(lines);
    //public void Prompt(string content) => GameManager.ui_Manager.Prompt(content);
    IEnumerator PerformFocusProcess()
    {
        controller.follow = false;
        yield return controller.Focus(targets, duration);
        GameManager.ui_Manager.Prompt(content);
        yield return new WaitUntil(ConversationOver);
        controller.follow = true;
    }
   
}
