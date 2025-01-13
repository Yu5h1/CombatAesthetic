using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Yu5h1Lib.Game.Character;
using Yu5h1Lib.EditorExtension;
using Yu5h1Lib;
using UnityEditorInternal;

[CustomEditor(typeof(AnimatorController2D))]
public class AnimatorController2DEditor : Controller2DEditor
{
    public AnimatorController2D contoller => targetObject as AnimatorController2D;

    //private ReorderableList skillsReorderableList;

    private void OnEnable()
    {
        var skillsProperty = serializedObject.FindProperty("_Skills");
        //skillsReorderableList = new ReorderableList(serializedObject, skillsProperty, true, true, true, true);
        //skillsReorderableList.drawHeaderCallback = (Rect rect) =>
        //{
        //    EditorGUI.LabelField(rect, "Skills");
        //};
        //skillsReorderableList.drawElementCallback = (Rect rect, int i, bool isActive, bool isFocused) =>
        //{
        //    var item = skillsProperty.GetArrayElementAtIndex(i);

        //    if (contoller.skills[i] && contoller.skills[i].incantation.IsEmpty())
        //        rect.width -= 25;

        //    EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), item, GUIContent.none);
        //    if (!contoller.skills[i] || !contoller.skills[i].incantation.IsEmpty())
        //        return;

            
        //    var toggled = i == contoller.indexOfSkill;

        //    var newToggled = GUI.Toggle(new Rect(rect.x + rect.width + 5, rect.y, 20, EditorGUIUtility.singleLineHeight),
        //                     toggled, "");
        //    if (newToggled != toggled)
        //    {
        //        contoller.indexOfSkill = i;
        //    }
        //};
    }

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

        //EditorGUI.BeginChangeCheck(); // 開始檢查是否有變更

        //skillsReorderableList.DoLayoutList();

        //if (EditorGUI.EndChangeCheck()) // 如果有變更
        //{
        //    serializedObject.ApplyModifiedProperties(); // 應用變更
        //}

        base.OnInspectorGUI();
    }
 
};