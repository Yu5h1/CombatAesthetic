using UnityEditor;
using Yu5h1Lib.Game.Character;
using Yu5h1Lib;
using UnityEditorInternal;
using System.Linq;
using UnityEditor.SceneManagement;
using Yu5h1Lib.EditorExtension;
using System.Collections.Generic;
using UnityEngine;

[CustomEditor(typeof(AnimatorCharacterController2D))]
public class AnimatorCharacterController2DEditor : Editor<AnimatorCharacterController2D>
{
    public AnimatorCharacterController2D contoller => targetObject;

    private ReorderableListEnhanced skillsList;

    private Dictionary<string, System.Action<SerializedProperty>> propertyDrawers;

    private GUIContent cachedContent = new GUIContent();

    private void OnEnable()
    {
        ReorderableListEnhanced.TryCreate(serializedObject, "_Skills", out skillsList);
        skillsList.drawElement += SkillsList_drawElement;

        propertyDrawers = new Dictionary<string, System.Action<SerializedProperty>>()
        {
            { "_Skills", DrawSkills }
        };
    }

    private void SkillsList_drawElement(Rect rect, int index, bool isActive, bool isFocused)
    {
        if (EditorApplication.isPlaying && targetObject.skillBehaviours.IsValid(index))
        {
            rect.x = rect.x + EditorGUIUtility.labelWidth - 20;
            targetObject.skillBehaviours[index].enable = EditorGUI.Toggle(rect, targetObject.skillBehaviours[index].enable);
        }
    
    }

    public override void OnInspectorGUI()
    {
        if (!contoller || PrefabStageUtility.GetCurrentPrefabStage() != null)
        {
            DrawDefaultInspector();
            return;
        }
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
        this.Iterate(OnDrawProperty);
    }

    private void OnDrawProperty(SerializedProperty property)
    {
        if (propertyDrawers.ContainsKey(property.name))
            propertyDrawers[property.name].Invoke(property);
        else
            EditorGUILayout.PropertyField(property);
    }
    private void DrawSkills(SerializedProperty property)
    {
        skillsList.DoLayoutList();

    }
};