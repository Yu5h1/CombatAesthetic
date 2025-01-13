using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Yu5h1Lib.EditorExtension;

[CustomEditor(typeof(LineRendererController))]
public class LineRendererControllerEditor : Editor<LineRendererController> {

    private void OnEnable()
    {
        if (EditorApplication.isPlaying)
            return;
    }
    public override void OnInspectorGUI()
   {
        
      //DrawDefaultInspector();
        if (this.Iterate(out SerializedProperty changedProperty))
        { 
         
        }
   }
    private void OnSceneGUI()
    {
        if (EditorApplication.isPlaying)
            return;
        targetObject.Refresh();
    }
}