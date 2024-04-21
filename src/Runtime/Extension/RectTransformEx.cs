
using UnityEngine;
using UnityEngine.UI;

public static class RectTransformEx
{
    public static bool TryGetGraphInChildren<Graph>(this RectTransform rt, string name, out Graph graph) where Graph : Graphic
    {
        graph = null;
        return rt.TryFind(name,out Transform t) && t.TryGetComponent(out graph);
    }
}