using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Yu5h1Lib.Graphic2D
{
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never), System.ComponentModel.Browsable(false)]
    public static class LineEx
    {
        public static void Draw(this Line l,Color color)
        {
#if UNITY_EDITOR
            Debug.DrawLine(l.begin, l.end, color);
    #endif
        }
        public static void Draw(this Line l)
        {
#if UNITY_EDITOR
            Debug.DrawLine(l.begin, l.end);
#endif
        }
    }

}