using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Yu5h1Lib.EditorExtension
{
    public class UnityEventDrawerEnhanced : UnityEditorInternal.UnityEventDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(position, property, label);
            
        }

        protected override void DrawEvent(Rect rect, int index, bool isActive, bool isFocused)
        {
            
            base.DrawEvent(rect, index, isActive, isFocused);
            GUI.Label(rect, "QQ");
        }
    } 
}
