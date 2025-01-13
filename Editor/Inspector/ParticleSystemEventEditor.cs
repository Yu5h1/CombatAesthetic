using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Yu5h1Lib.EditorExtension;

[CustomEditor(typeof(ParticleSystemEvent))]
public class ParticleSystemEventEditor : Editor<ParticleSystemEvent> {
   
   public override void OnInspectorGUI()
   {
		DrawDefaultInspector();
   }
};