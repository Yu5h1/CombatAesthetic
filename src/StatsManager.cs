using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static Yu5h1Lib.GameManager.IDispatcher;
using Controller2D = Yu5h1Lib.Game.Character.Controller2D;
using PlayerInput = Yu5h1Lib.Game.Character.PlayerInput;

public class StatsManager : MonoBehaviour
{
    public static StatsManager instance => PoolManager.statsManager;
    public PoolManager poolManager => PoolManager.instance;

    public UIStat uiStatSource => Resources.Load<UIStat>("UI/BaseStatBar");

    public Dictionary<GameObject, StatProperty> CharactersMap = new Dictionary<GameObject, StatProperty>();
    public List<StatProperty> Stats = new List<StatProperty>();
    public StatProperty Target { get; private set; }

    private void Awake()
    {
        
    }
    // Start is called before the first frame update
    void Start()
    {
        poolManager.Add_UI(uiStatSource.gameObject,5);

        CharactersMap = FindObjectsByType<Controller2D>(FindObjectsInactive.Include, FindObjectsSortMode.None).
            Select(c => {
                //c.Init();
                var result = new StatProperty(c);
                if (c.tag == "Player")
                    Target = result;
                return result;
            }).ToDictionary(info => info.characterController.gameObject, info => info);
        Stats.AddRange(CharactersMap.Select(c => c.Value));
        if (Target != null)
        {
            cameraController.target = Target.characterController.transform;
            if (SceneController.IsStartScene)
                Target.SetUIVisible(false);
            else
            {
                Target.characterController.host = Resources.Load<PlayerInput>("PlayerInput");
                Target.CreateDefaultVisualItems();
                Target.Stat_UI.SetParent(transform,false);
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        for (int i = Stats.Count - 1; i >= 0; i--)
        {
            var current = Stats[i];
            current.UpdateVisualItems();
            if (current.DespawnReason != DespawnReason.None)
            {
                OnCharacterDespawn(current);
                Stats.RemoveAt(i);
            }
        }
    }
    //public void SpawnUI_stat(AttributeType attribute)
    //{
    //    PoolManager.instance.Spawn(,);
    //}
    public void OnCharacterDespawn(StatProperty characterProperty)
    {
        if (characterProperty == null)
        {
            Debug.LogWarning("Why your characterProperty is null ?");
            return;
        }
        var controller = characterProperty.characterController;
        if (controller.tag == "Player")
        {
            controller.enabled = false;
            controller.animator.Play("Fail");
            controller.rigidbody.simulated = false;
            //UIController.Fadeboard_UI.FadeIn(Color.black,true);
            PoolManager.canvas.sortingLayerName = "Back";
            cameraController.FadeIn("Back",1);
            
            uiManager.LevelSceneMenu.FadeIn(5);

            //foreach (var item in CharactersForUpdate)
            //    item.characterController.enabled = false;
        }
        else
        {
            // npc ring out
            characterProperty.characterController.gameObject.SetActive(false);
        }
        characterProperty.characterController.host = null;
        Debug.Log($"{characterProperty.characterController.gameObject.name} was defeated because of {characterProperty.DespawnReason}.");
    }
    public void Despawn(GameObject obj, DespawnReason reason)
    {        
//#if UNITY_EDITOR
//        if (!UnityEditor.EditorApplication.isPlaying)
//            return;
//#endif
        if (CharactersMap.ContainsKey(obj))
            CharactersMap[obj].DespawnReason = DespawnReason.OutOfBounds;

        //Debug.Log($"Despawn {obj.name}.");
    }
    private void OnDestroy()
    {
        foreach (var item in CharactersMap)
            item.Value.Destory();
    }

}
