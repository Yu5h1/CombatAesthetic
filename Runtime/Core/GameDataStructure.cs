using UnityEngine;

[System.Serializable]
public struct AttributeStat
{
    public float max;
    public float current;
    public float recovery;
    public float normal => current.GetNormal(max);
    public bool IsDepleted => current <= 0;
    public bool IsFull => current >= max;
    public static float DefaultMax = 100;

    public static readonly AttributeStat Default = new AttributeStat()
    {
        current = DefaultMax,
        max = DefaultMax,
        recovery = 10
    };
    public void Init() => current = max;
    public void Init(float max)
    {
        this.max = max;
        Init();
    }
    public void Affect(AffectType affectType, float amount) => current += amount * (int)affectType;
    public override string ToString() => $"{current}";
}
[System.Serializable]
public struct EnergyInfo
{
    public AttributeType attributeType;
    public float amount;
}
[System.Flags]
public enum AttributeType
{
    None       = 0,
    Health     = 1 << 0,
    Mana       = 1 << 1,
    Stamina    = 1 << 2,
    //All        = Health | Mana | Stamina
}
public enum AffectType
{
    POSITIVE = 1,  
    NEGATIVE = -1, 
    NEUTRAL = 0    
}
[System.Flags]
public enum DefeatedReason
{
    None = 0,
    Exhausted = 1 << 0,
    OutOfBounds = 1 << 1,
}
public static class GameEnumTypeUtility
{
    public static Color GetColor(this AttributeType type) => type switch
    {
        AttributeType.None => Color.black,
        AttributeType.Health => Color.red,
        AttributeType.Mana => Color.blue,
        AttributeType.Stamina => Color.yellow,
        _ => Color.white
    };
}