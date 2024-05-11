using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using Yu5h1Lib;

public class TriggerEvent2D : EventMask2D
{
    public UnityEvent<Collider2D> OnTriggerEnter2DEvent;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!Validate(other.gameObject))
            return;
        OnTriggerEnter2DEvent?.Invoke(other);
    }
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
public abstract class TriggerEvent2D<T> : MonoBehaviourEnhance where T : Component
{
    [Serializable]
    public class TEvent : UnityEvent<T> { }
    public TEvent OnTriggerEnter2DEvent;
    public TEvent OnTriggerExit2DEvent;
    protected new Collider collider; 
    private void Reset()
    {
        CheckTriggerState();
    }
    private void OnTriggerEnter2D(Collider2D other) => OnTriggerEntered2D(other.GetComponent<T>());
    private void OnTriggerExit2D(Collider2D collision) => OnTriggerExited2D(collision.GetComponent<T>());

    protected virtual bool OnTriggerEntered2D(T component) => IsValid(component);
    protected virtual bool OnTriggerExited2D(T component) => IsValid(component);

    protected bool IsValid(T component)
        => component != null && (string.IsNullOrEmpty(tag) || tag.Equals("Untagged") || component.gameObject.tag.Equals(tag));
    protected void CheckTriggerState()
    {

        for (int i = 0; i < gameObject.GetComponentCount(); i++)
        {
            var component = gameObject.GetComponentAtIndex(i);
            if (component.GetType().IsSubclassOf(typeof(Collider2D)))
            {
                var collider = (Collider2D)component;
                if (!collider.isTrigger)
                {
                    collider.isTrigger = true;
                    Debug.LogWarning($"[{name}({component.GetType()})] was set to false for the 'isTrigger' property. This component is now changing it to true.");
                }
            }
        }
    }

}