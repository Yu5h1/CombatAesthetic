using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Yu5h1Lib.EditorExtension;

[CustomEditor(typeof(CharacterSMB))]
public class CharacterSMBEditor : Editor<CharacterSMB> {
   public override void OnInspectorGUI()
   {
      DrawDefaultInspector();
   }
}