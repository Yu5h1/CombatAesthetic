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

    /// <summary>
    /// stats.Length
    /// </summary>
    public int Count => stats.Length;

    #region Events
    [SerializeField]
    private UnityEvent<AttributeType> _onAffect;
    public event UnityAction<AttributeType> onAffect
    {
        add => _onAffect.AddListener(value);
        remove => _onAffect.RemoveListener(value);
    }
    [SerializeField]
    private UnityEvent<AttributeType> StatDepletedEvent;
    public event UnityAction<AttributeType> StatDepleted
    {
        add => StatDepletedEvent.AddListener(value);
        remove => StatDepletedEvent.RemoveListener(value);
    } 
    #endregion

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

    public bool IsEmpty(AttributeType type) { 
        if (TryGetState(type,out AttributeStat stat))
            return stat.current < 0;
        return true;
    }

    private UI_Attribute _ui;

    public UI_Attribute ui
    {
        get => _ui;
        set 
        {
            if (_ui == value)
                return;
            _ui = value;
            if (_ui)
                _ui.Prepare(this);
        }
    }




    /// <summary>
    /// stop update attributes while affecting
    /// </summary>
    private bool affected;
    public void Reset()
    {
        attributes = AttributeType.Health;
    }


    internal void Init()
    {
        Keys = attributes.SeparateFlags().Select(flag => $"{flag}").ToArray();
        if (stats.Length != Keys.Length)
        {
            int previouseStatCount = stats.Length;
            System.Array.Resize(ref _stats, Keys.Length);
            if (previouseStatCount < stats.Length)
                for (int i = previouseStatCount; i < stats.Length; i++)
                    stats[i] = AttributeStat.Default;
        }
    }
    private void OnEnable()
    {
        for (int i = 0; i < stats.Length; i++)
            stats[i].Init();        
    }
    void Update()
    {
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
            ui?.uI_stats[i]?.UpdateStat(stats[i]);
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
            var index = Keys.IndexOf(flag.ToString());
            if (index < 0)
                continue;
            stats[index].Affect(affectType, amount);
            OnAffected(flag);
            if (stats[index].IsDepleted)
            {
                OnStatDepleted(flag);
                DepletedTypes |= flag;
            }
        }
        return DepletedTypes;
    }
    private void OnAffected(AttributeType flag)
    {
        _onAffect?.Invoke(flag);
        var index = Keys.IndexOf(flag.ToString());
        if (index < 0)
            return;
        ui?.uI_stats[index]?.UpdateStat(stats[index]);
    }
    private void OnStatDepleted(AttributeType flag)
    {
        StatDepletedEvent?.Invoke(flag);
        if (!enabled)
            return;
        if (!flag.HasFlag(AttributeType.Health))
            return;
        #region unorganized implmentation
        if (tag == "Player")
        {
            GetComponent<SpriteRenderer>().sortingLayerName = "Front";
            ui?.FadeOut(0.3f);
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

    private void OnDisable()
    {
        _onAffect.RemoveAllListeners();
        StatDepletedEvent.RemoveAllListeners();
    }
}
