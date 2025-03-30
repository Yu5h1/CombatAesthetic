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

    public AttributeStat? this[AttributeType type]
    {
        get {
            var index = (int)type;
            return stats.IsValid(index) ? stats[index] : null;
        }
    }

    [SerializeField, ReadOnly]
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
            _UI_Changed?.Invoke(_ui);
        }
    }

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
    [SerializeField]
    private UnityEvent<UI_Attribute> _UI_Changed;
    public event UnityAction<UI_Attribute> UI_Changed
    {
        add => _UI_Changed.AddListener(value);
        remove => _UI_Changed.RemoveListener(value);
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
    [SerializeField,ReadOnly]
    private bool _exhausted;
    public bool exhausted => _exhausted;    
    public bool IsEnough(string key, float amount) => TryGetIndex(key, out int index) && stats[index].current >= amount;

    public bool IsEmpty(AttributeType type) { 
        if (TryGetState(type,out AttributeStat stat))
            return stat.current < 0;
        return true;
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
        if (exhausted)
            return;
        if (affected)
        {
            affected = false;
            return;
        }
        if (!exhausted && stats[0].IsDepleted)
            OnStatDepleted(AttributeType.Health);

        for (int i = 0; i < stats.Length; i++)
        {
            if (stats[i].recovery == 0 || stats[i].IsFull)
                continue;
            stats[i].current += stats[i].recovery * Time.deltaTime;
            ui?.uI_stats[i]?.Refresh(stats[i]);
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
    public AttributeType Affect(AffectType affectType, params EnergyInfo[] infos)
    {
        var DepletedTypes = AttributeType.None;
        foreach (var info in infos)
            DepletedTypes |= Affect(info.attributeType, affectType, info.amount);
        return DepletedTypes;
    }

    private void OnAffected(AttributeType flag)
    {
        _onAffect?.Invoke(flag);
        var index = Keys.IndexOf(flag.ToString());
        if (index < 0)
            return;
        ui?.uI_stats[index]?.Refresh(stats[index]);
    }
    private void OnStatDepleted(AttributeType flag)
    {
        if (!enabled)
            return;
        StatDepletedEvent?.Invoke(flag);
        if (flag.HasAnyFlags(AttributeType.Health))
            _exhausted = true;
#if UNITY_EDITOR
        ///unorganized implmentation    
        if (flag.HasAnyFlags(AttributeType.Health))
        {
            enabled = false;
            Debug.Log($"{gameObject.name} was defeated because of {DefeatedReason.Exhausted}.");
        }
#endif
    }
    public bool Validate(Dictionary<string,int> cost)
    {
#if UNITY_EDITOR
        "stats is empty".printWarningIf(stats.IsEmpty());
#endif
        if (stats.IsEmpty())
            return false;
        foreach (var item in cost)
        {
            if (!IsEnough(item.Key, item.Value))
            {
#if UNITY_EDITOR
                $"{name} : Not enough {item.Key}".printWarning();
#endif
                return false;
            }
        }
        return true;
    }

    private void OnDestroy()
    {
        _onAffect.RemoveAllListeners();
        StatDepletedEvent.RemoveAllListeners();
    }
    public IEnumerator Affect(int index, float interval)
    {
        while (!stats[index].IsFull)
        {
            stats[index].current += stats[index].recovery * Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        routines[index] = null;
        yield return null;
    }
}
