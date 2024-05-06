using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UI_Dialog_TMP))]
public class UI_DialogInspector : Editor<UI_Dialog_TMP>
{
    //SerializedObject serializedTarget;
    void OnEnable()
    {
        //serializedTarget = new SerializedObject(target);
    }
    public override void OnInspectorGUI()
    {
        //serializedTarget.Update();
        base.OnInspectorGUI();
        if (GUILayout.Button("Add element from Content"))
        {
            targetObject.AddElementFromContent();
            EditorUtility.SetDirty(target);
        }

    }
}
