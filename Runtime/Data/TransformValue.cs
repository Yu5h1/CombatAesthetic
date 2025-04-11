using UnityEngine;

[System.Serializable]
public struct TransformValue 
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;

    public readonly static TransformValue Default = new TransformValue()
    {
        position = Vector3.zero,
        rotation = Quaternion.identity,
        scale = Vector3.one,
    };
}
