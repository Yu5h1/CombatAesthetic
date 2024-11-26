using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Yu5h1Lib;

public class TeleportGate2D : PlayerEvent2D
{
    #region Static
    public static Dictionary<string, bool> GateStates = new Dictionary<string, bool>();
    #endregion

    public Vector2 destination;
    public int sceneIndex = -1;

    public bool enableTrunOffAfterTriggered = true;
    public bool AllowRecordStatus;

    private void Reset()
    {
        destination = transform.position + new Vector3(1,1,0);
    }
    private void Awake()
    {
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

        if (enableTrunOffAfterTriggered)
            gameObject.SetActive(false);
        if (AllowRecordStatus)
            GateStates[name] = gameObject.activeSelf;

        // teleport in current scene
        if (sceneIndex < 0)
            other.transform.position = destination;
        else
        {
            SceneController.startPosition = destination;
            SceneController.LoadScene(sceneIndex);
        }
    }
    private void OnDisable()
    {
      
    }
    private void OnDrawGizmosSelected()
    {
        var color = Gizmos.color;
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(destination, 0.5f);
        Gizmos.color = color;
    }
}
