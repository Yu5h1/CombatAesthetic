using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Yu5h1Lib.EditorExtension;

[CustomEditor(typeof(PoolManager))]
public class PoolManagerEditor : Editor<PoolManager> {
   public override void OnInspectorGUI()
   {
        DrawDefaultInspector();
        EditorGUILayout.HelpBox(
@$"Active:{MeshPool.container.CountActive}
Inactive:{MeshPool.container.CountInactive}",MessageType.Info);
    }
}