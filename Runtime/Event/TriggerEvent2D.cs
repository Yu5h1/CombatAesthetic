using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using Yu5h1Lib;

public class TriggerEvent2D : EventMask2D
{
    public UnityEvent<Collider2D> OnTriggerEnter2DEvent;
    public UnityEvent<Collider2D> TriggerExit2D;

    [SerializeField,Header("limit of trigger")]
    private int _count = -1;
    public int count => _count;
    public int counter { get; set; }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!Validate(other.gameObject))
            return;

        if (count > 0)
        {
            counter++;
            if (count > 0 && counter >= count)
                enabled = false;
        }
        OnTriggerEnter2DEvent?.Invoke(other);
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!Validate(other.gameObject))
            return;
        TriggerExit2D?.Invoke(other);
    }
}
public abstract class TriggerEvent2D<T> : MonoBehaviour where T : Component
{
    [Serializable]
    public class TEvent : UnityEvent<T> { }
    public TEvent OnTriggerEnter2DEvent;
    public TEvent OnTriggerExit2DEvent;

#pragma warning disable 0109
    protected new Collider collider;
#pragma warning restore 0109

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