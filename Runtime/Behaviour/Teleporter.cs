using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using Yu5h1Lib;

public class Teleporter : PlayerEvent2D
{
    #region Static
    public static Dictionary<string, bool> GateStates = new Dictionary<string, bool>();
    #endregion


    public bool loadSceneOnly;
    public int sceneIndex = -1;

    [ContextMenuItem("Reset", nameof(ResetDestination))]
    public Vector2 destination;

    public bool TrunOffAfterTriggered = true;
    public bool AllowRecordStatus;

    public UnityEvent<Collider2D> triggerEnter;


    private void Reset()
    {
        destination = transform.position + new Vector3(1,1,0);
    }
    private void Awake()
    {
    }
    private void Start()
    {
        foreach (var c in GetComponents<Collider2D>())
            c.isTrigger = true;
    }
    private void OnEnable()
    {
        if (GateStates.ContainsKey(name))
            gameObject.SetActive(GateStates[name]);
            
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!Validate(other))
            return;

        if (TrunOffAfterTriggered)
            gameObject.SetActive(false);
        if (AllowRecordStatus)
            GateStates[name] = gameObject.activeSelf;

        // teleport in current scene
        if (sceneIndex < 0)
            other.transform.position = destination;
        else
        {
            if (!loadSceneOnly)
            {
                SceneController.startPosition = destination;
                SceneController.startRotation = other.transform.rotation;
            }
            SceneController.LoadScene(sceneIndex);
        }
        triggerEnter?.Invoke(other);
    }
    private void OnDisable()
    {
      
    }

    #region MyRegion
    private void ResetDestination() => destination = transform.position;
    #endregion

}
