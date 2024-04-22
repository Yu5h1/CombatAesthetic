using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;


[DisallowMultipleComponent]
public class AttributeStatBehaviour : MonoBehaviour
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

    public event UnityAction<AttributeType> StatDepleted;

    public bool TryGetIndex(string key, out int index) => (index = Keys.IndexOf(key)) >= 0;
    public bool TryGetIndex(AttributeType type, out int index) => TryGetIndex($"{type}", out index);
    public bool IsEnough(string key, float amount) => TryGetIndex(key, out int index) && stats[index].current >= amount;

    public UI_statbar[] uI_Statbars { get; private set; }

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
                    stats[i] = AttributeStat.Default;
            }
        }
    }
    private void OnEnable()
    {
        for (int i = 0; i < stats.Length; i++)
            stats[i].Init();
    }
    void Update()
    {
        for (int i = 0; i < stats.Length; i++)
        {
            if (stats[i].IsFull)
                continue;
            stats[i].current += stats[i].recovery * Time.deltaTime;
        }
    }
    public void Affect(AttributeType attributeType, AffectType affectType, float amount)
    {
        foreach (var flag in attributeType.SeparateFlags())
        {
            var index = Keys.IndexOf($"{flag}");
            if (index < 0)
                continue;
            stats[index].Affect(affectType, amount);
            if (stats[index].IsDepleted)
                StatDepleted?.Invoke(flag);
        }
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
    public void Affect(AffectType affectType,params EnergyInfo[] infos)
    {
        foreach (var info in infos)
            Affect(info.attributeType, affectType, info.amount);
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
    public StatProperty.VisualItem[] CreateVisualItems(RectTransform parent, Vector2 size, bool UpDown)
        => Keys.Select((key, order) => new StatProperty.VisualItem(parent, System.Enum.Parse<AttributeType>(key), order, size, UpDown)).ToArray();


    public void GetUI_StatsBar()
    {
        //uI_Statbars = new UI_statbar[Keys.Length];
        //    PoolManager.instance.Spawn("BaseStatBar");
    }

    #endregion
}
