using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Yu5h1Lib.EditorExtension;

[CustomEditor(typeof(ParticleSystemEvent2D))]
public class ParticleSystemEventEditor : Editor<ParticleSystemEvent2D> {
   
   public override void OnInspectorGUI()
   {
		DrawDefaultInspector();
   }
};