using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Yu5h1Lib;
using Yu5h1Lib.Game.Character;

[DisallowMultipleComponent]
public class AttributeBehaviour : MonoBehaviour
{
    private CameraController cameraController => CameraController.instance;
    public static readonly AttributeType[] AttributeTypes = EnumEx.GetValues<AttributeType>();
    [SerializeField]
    private AttributeType attributes = AttributeType.Health;
    [SerializeField]
    private AttributeStat[] _stats;
    public AttributeStat[] stats { get => _stats; private set => _stats = value; }
    public string[] Keys { get; private set; }
    public Coroutine[] routines { get; private set; }

    public bool VisualizeStat;
    public bool IsEmpty => attributes == AttributeType.None;
    public int Count => stats.Length;

    [SerializeField]
    private UnityEvent<AttributeType> StatDepletedEvent;
    public event UnityAction<AttributeType> StatDepleted {
        add => StatDepletedEvent.AddListener(value);
        remove => StatDepletedEvent.RemoveListener(value);
    }
    public bool TryGetIndex(string key, out int index) => (index = Keys.IndexOf(key)) >= 0;
    public bool TryGetIndex(AttributeType type, out int index) => TryGetIndex($"{type}", out index);
    public bool TryGetState(AttributeType type, out AttributeStat stat) {
        stat = default(AttributeStat);
        if (TryGetIndex($"{type}", out int index))
        {
            stat = stats[index];
            return true;
        }
        return false;
    } 

    public bool IsEnough(string key, float amount) => TryGetIndex(key, out int index) && stats[index].current >= amount;

    public UI_Attribute ui { get; set; }

    private bool affected;
    public void Reset()
    {
        attributes = AttributeType.Health;
    }
    private void Awake()
    {
        Keys = attributes.SeparateFlags().Select(flag => $"{flag}").ToArray();
        if (stats.Length != Keys.Length)
        {
            int previouseStatCount = stats.Length;
            System.Array.Resize(ref _stats, Keys.Length);
            if (previouseStatCount < stats.Length)
            {
                for (int i = previouseStatCount; i < stats.Length; i++)
                {
                    stats[i] = AttributeStat.Default;
                }
            }
        }
    }
    private void Start()
    {
    }
    private void OnEnable()
    {
        for (int i = 0; i < stats.Length; i++)
            stats[i].Init();
    }
    void Update()
    {
        #region UI
        if (ui != null)
            ui.UpdateAttribute(this);
        #endregion
        if (affected)
        {
            affected = false;
            return;
        }
        for (int i = 0; i < stats.Length; i++)
        {
            if (stats[i].IsFull)
                continue;
            stats[i].current += stats[i].recovery * Time.deltaTime;
        }
    }
    /// <summary>
    /// Returns depleted attributeType. 
    /// </summary>
    public AttributeType Affect(AttributeType attributeType, AffectType affectType, float amount)
    {
        affected = true;
        var DepletedTypes = AttributeType.None;
        foreach (var flag in attributeType.SeparateFlags())
        {
            var index = Keys.IndexOf($"{flag}");
            if (index < 0)
                continue;
            stats[index].Affect(affectType, amount);
            if (ui != null)
                ui.UpdateAttribute(this);
            if (stats[index].IsDepleted)
            {
                OnStatDepleted(flag);
                DepletedTypes |= flag;
            }
        }
        return DepletedTypes;
    }
    private void OnStatDepleted(AttributeType flag)
    {
        StatDepletedEvent?.Invoke(flag);
        if (!flag.HasFlag(AttributeType.Health))
            return;
        if (!enabled)
            return;
        #region unorganized implmentation
        if (tag == "Player")
        {

            //UIController.Fadeboard_UI.FadeIn(Color.black,true);
            ui.FadeOut(0.3f);
            PoolManager.canvas.sortingLayerName = "Back";
            CameraController.instance.FadeIn("Back", 1);
            GameManager.ui_Manager.LevelSceneMenu.FadeIn(5);
            
        }
        Debug.Log($"{gameObject.name} was defeated because of {DefeatedReason.Exhausted}.");
        enabled = false;
        #endregion
    }
    public IEnumerator Affect(int index,float interval) {
        while (!stats[index].IsFull)
        {
            stats[index].current += stats[index].recovery * Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        routines[index] = null;
        yield return null;
    }    
    public AttributeType Affect(AffectType affectType,params EnergyInfo[] infos)
    {
        var DepletedTypes = AttributeType.None;
        foreach (var info in infos)
            DepletedTypes |= Affect(info.attributeType, affectType, info.amount);
        return DepletedTypes;
    }

    public bool Validate(Dictionary<string,int> cost)
    {
        if (stats.IsEmpty())
            return false;
        foreach (var item in cost)
            if (!IsEnough(item.Key, item.Value))
                return false;
        return true;
    }

    #region UI
    public void GetUI_StatsBar()
    {
        //uI_Statbars = new UI_statbar[Keys.Length];
        //    PoolManager.instance.Spawn("BaseStatBar");
    }

    #endregion
}
