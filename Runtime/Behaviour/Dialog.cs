using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Yu5h1Lib;

public class Dialog : MonoBehaviour
{
	public float delay;
    public float speed = 0.05f;
    [TextArea(1,10)]
	public string content;

    public UnityEvent ConversationOver;

	public void Perform()
	{
		StartCoroutine(DelayPrompt(delay));
	}
    private IEnumerator DelayPrompt(float delay)
    {
        GameManager.SetPlayerControllable(false);
        if (delay > 0)
            yield return new WaitForSeconds(delay);
        GameManager.ui_Manager.Dialog_UI.Perform(content,0,speed);
        yield return new WaitUntil(GameManager.NotSpeaking);
        ConversationOver?.Invoke();
        GameManager.SetPlayerControllable(true);
    }
}
