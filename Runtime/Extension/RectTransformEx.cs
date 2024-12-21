
using UnityEngine;
using UnityEngine.UI;
using Yu5h1Lib;

public static class RectTransformEx
{
    public static bool TryGetGraphInChildren<Graph>(this RectTransform rt, string name, out Graph graph) where Graph : Graphic
    {
        graph = null;
        return rt.TryFind(name,out Transform t) && t.TryGetComponent(out graph);
    }
    public static void SetSize(this RectTransform r,float? width = null, float? height = null)
        => r.sizeDelta = new Vector2(width ?? r.sizeDelta.x, height ?? r.sizeDelta.y);
}