using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(UI_Statbar))]
public class UI_statbarInspector : Editor<UI_Statbar>
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (this.TrySlider(out float val, targetObject.fill.fillAmount, 0, 1))
        {
            targetObject.fill.fillAmount = val;
            EditorApplication.QueuePlayerLoopUpdate();
        }
    }
}
