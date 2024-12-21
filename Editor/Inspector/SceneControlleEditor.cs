using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Yu5h1Lib.EditorExtension;

[CustomEditor(typeof(SceneController))]
public class SceneControlleEditor : Editor<SceneController>
{
    private void OnSceneGUI()
    {
        Handles.SphereHandleCap(0, targetObject.defaultStartPoint,Quaternion.identity,1,EventType.Repaint);
        var pos = Handles.PositionHandle(targetObject.defaultStartPoint,Quaternion.identity);
        if (pos != targetObject.defaultStartPoint)
            targetObject.defaultStartPoint = pos;
    }
}
