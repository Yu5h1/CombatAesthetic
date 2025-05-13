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
    public void Init() => current = current < max ? max : current;
    public void Init(float max)
    {
        this.max = max;
        Init();
    }
    public void Affect(AffectType affectType, AttributePropertyType propertyType, float amount)
    {
        switch (propertyType)
        {
            case AttributePropertyType.Max:
                max += amount * (int)affectType;
                break;
            case AttributePropertyType.Current:
                current += amount * (int)affectType;
                break;
            case AttributePropertyType.RegenRate:
                recovery += amount * (int)affectType;
                break;
            default:
                break;
        }
    } 
    public override string ToString() => $"{current}";
}
//[System.Serializable]
//public struct AttributeTypeStat 
//{
//    public string Name => type.ToString();
//    public AttributeType type;
//    public AttributeStat attribute;
//}

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
    Red        = 1 << 3,
    Yellow     = 1 << 4,
    Blue       = 1 << 5,
    //All        = Health | Mana | Stamina
}
public enum AttributePropertyType
{
    Max,
    Current,
    RegenRate
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