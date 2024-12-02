using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yu5h1Lib.EditorExtension;
using UnityEditor;
using UnityEngine.PlayerLoop;

[CustomEditor(typeof(UI_StatAnimation))]
public class UI_StatAnimationEditor : Editor<UI_StatAnimation>
{
    AttributeStat testStat = new AttributeStat();
    private void OnEnable()
    {
        if (EditorApplication.isPlaying)
            return;
        testStat = AttributeStat.Default;

    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (EditorApplication.isPlaying)
            return;
        var val = EditorGUILayout.Slider("SimulateValue", testStat.normal, 0, 1);
        if (testStat.normal != val)
        {
            testStat.current = val * testStat.max;
            targetObject.UpdateStat(testStat);
        }
    }
}
