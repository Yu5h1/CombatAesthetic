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

    private void Start() {}

    public void Perform()
	{
        if (!isActiveAndEnabled || content.IsEmpty())
            return;
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

    public Color color = Color.white;
    public float lifeTime = 1;
    public float fadeInTime = 1;
    public float fadeOutTime = 1f;

    public void ShowInnerThoughts(string text)
    {
        GameManager.ui_Manager.textPerformance?.ShowInnerThoughts(text, color, fadeInTime, lifeTime);
    }
}
