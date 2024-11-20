using UnityEngine;

namespace Yu5h1Lib
{
    public interface ITransform2D
    {
        Vector2 up       { get; }
        Vector2 down     { get; }
        Vector2 left     { get; }
        Vector2 right    { get; }
    }
}
