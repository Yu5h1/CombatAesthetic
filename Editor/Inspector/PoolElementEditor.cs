using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Yu5h1Lib.EditorExtension;
using UnityEditor.Graphs;

[CustomEditor(typeof(PoolElement))]
public class PoolElementEditor : Editor<PoolElement> {
   public override void OnInspectorGUI()
   {
        DrawDefaultInspector();
        if (targetObject.map != null)
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Source", targetObject.map.Source, typeof(Component), true);
            EditorGUILayout.ObjectField("Source", targetObject.map.Element, typeof(Component), true);
            EditorGUI.EndDisabledGroup();
        }
        else
            EditorGUILayout.LabelField("Map is Null");
   }
}