using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Yu5h1Lib.Game.Character;
using Yu5h1Lib.EditorExtension;
using Yu5h1Lib;
using UnityEditorInternal;

[CustomEditor(typeof(AnimatorCharacterController2D))]
public class AnimatorCharacterController2DEditor : Controller2DEditor
{
    public AnimatorCharacterController2D contoller => targetObject as AnimatorCharacterController2D;

    public override void OnInspectorGUI()
    {
        if (!contoller)
            return;
        serializedObject.Update();
        if (EditorApplication.isPlaying && targetObject.isActiveAndEnabled)
        {
            EditorGUILayout.HelpBox($@"
contoller.InputMovement:{contoller.InputMovement},
IsActing:{contoller.IsActing}
IsInteracting:{contoller.IsInteracting}
IsGrounded: {contoller.IsGrounded}
Floatable:{contoller.Floatable}
GravityDirection:{contoller.gravityDirection},
eulerAngles:{contoller.transform.eulerAngles},
forward:{contoller.transform.forward}
forward.z:{contoller.transform.forward.z}
", MessageType.Info); //FallingTimeElapsed: { contoller.FallingTimeElapsed}
        }
 

        base.OnInspectorGUI();
    }
 
};