using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Yu5h1Lib.EditorExtension;

namespace Yu5h1Lib
{
    [CustomEditor(typeof(CheckPoint))]
    public class CheckPointEditor : Editor<CheckPoint>
    {
        private Transform transform => targetObject.transform;
        private Collider2D collider;
        private void OnEnable()
        {
            collider = transform.GetComponent<Collider2D>();
        }
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        }
        private void OnSceneGUI()
        {
            if (transform.position == CheckPoint.Position)
                Handles.Label(collider ? collider.bounds.Top() : transform.position, "Check Point");
        }
    } 
}