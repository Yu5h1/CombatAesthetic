using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Yu5h1Lib.Game.Character;

[CustomEditor(typeof(AnimatorController2D))]
public class AnimatorController2DEditor : Controller2DEditor {
   public AnimatorController2D contoller => targetObject as AnimatorController2D;

   public override void OnInspectorGUI()
   {
        if (!contoller)
            return;
        if (EditorApplication.isPlaying)
        {
            EditorGUILayout.HelpBox($@"
contoller.InputMovement:{contoller.InputMovement},
IsActing:{contoller.IsActing}
IsInteracting:{contoller.IsInteracting}
IsGrounded: {contoller.IsGrounded}
Floatable:{contoller.Floatable}
GravityDirection:{contoller.gravityDirection},

", MessageType.Info); //FallingTimeElapsed: { contoller.FallingTimeElapsed}
        }
        

        base.OnInspectorGUI();
   }
};