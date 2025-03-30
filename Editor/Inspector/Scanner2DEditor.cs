using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Yu5h1Lib.EditorExtension;
using UnityEditorInternal;
using Yu5h1Lib.Runtime;

[CustomEditor(typeof(Scanner2D)),CanEditMultipleObjects]
public class Scanner2DEditor : Editor<Scanner2D>
{
    private Transform transform => targetObject.transform;

    private bool simulate;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var simulateChecked = GUILayout.Toggle(simulate, "Simulate", "Button");
        if (simulateChecked != simulate)
        {
            simulate = simulateChecked;
            if (simulate)
            {
                targetObject.Init();
            }
        }
        
    }
    private void OnSceneGUI()
    {
        if (simulate)
        {
            targetObject.Scan(out RaycastHit2D hit);
        }
        if (!InternalEditorUtility.GetIsInspectorExpanded(target) || !targetObject.isActiveAndEnabled)
            return;
        
            if (targetObject.useCircleCast)
            DebugUtil.DrawCircle(transform.position, Quaternion.identity, targetObject.distance);
    }
}