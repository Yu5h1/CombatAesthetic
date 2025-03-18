using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using Yu5h1Lib;
using Yu5h1Lib.Game.Character;

public class Teleporter : PlayerEvent2D
{
    #region Static
    public static Dictionary<string, bool> GateStates = new Dictionary<string, bool>();
    #endregion

    public bool loadSceneOnly;
    [BuildScene]
    public int sceneIndex = -1;

    [ContextMenuItem("Reset", nameof(ResetDestination))]
    public Vector2 destination;
    public Teleporter TeleporterExit;

    public bool TrunOffAfterTriggered = true;
    public bool AllowRecordStatus;

    public UnityEvent<Collider2D> triggerEnter;

    [ReadOnly]
    public List<Collider2D> ignores = new List<Collider2D>();

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
        if (ignores.Contains(other))
        {
            ignores.Remove(other);
            return;
        }
        if (!Validate(other))
            return;

        if (TrunOffAfterTriggered)
            gameObject.SetActive(false);
        if (AllowRecordStatus)
            GateStates[name] = gameObject.activeSelf;

        // teleport in current scene
        if (sceneIndex < 0)
        {
            if (TeleporterExit != null)
            {
                TeleporterExit.ignores.Add(other);
                if (other.CompareTag("Player"))
                    GameManager.MovePlayer(TeleporterExit.destination);
                else
                    other.transform.position = TeleporterExit.transform.position;
            }
            else
            {
                if (other.CompareTag("Player"))
                    GameManager.MovePlayer(destination);
                else
                    other.transform.position = destination;
            }
        }
        else
        {
            SceneController.startPosition = loadSceneOnly ? null : destination;
            if (!loadSceneOnly)
                SceneController.startRotation = other.transform.rotation;
            SceneController.LoadScene(sceneIndex);
        }
        triggerEnter?.Invoke(other);
    }
    private void OnDisable()
    {
      
    }

    #region Methods
    private void ResetDestination() => destination = transform.position;
    #endregion

    private static HashSet<CharacterController2D> teleportingCharacters = new HashSet<CharacterController2D>();
    public static bool MoveCharacter(CharacterController2D character, Vector2 pos, Quaternion? rot = null)
    {
        if (!character || !character.gameObject.IsBelongToActiveScene())
            return false;
        teleportingCharacters.Add(character);
        character.rigidbody.simulated = false;
        character.transform.position = pos;
        if (rot != null)
            character.transform.rotation = rot.Value;
        character.rigidbody.simulated = true;
        teleportingCharacters.Remove(character);
        return true;
    }
    public static bool IsTeleporting(CharacterController2D character) => teleportingCharacters.Contains(character);
    public static bool IsTeleporting(Transform t) => teleportingCharacters.Any(c => c.transform == t);

}
