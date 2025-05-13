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
        SkillBehaviour b = null;
        if (targetObject.skillBehaviours.IsValid(index) && targetObject.skillBehaviours[index] != null)
            b = targetObject.skillBehaviours[index];

        bool enableSkill = b == null ? true : b.enable;
        rect.x = rect.x + EditorGUIUtility.labelWidth - 20;
        using (new EditorGUI.DisabledScope(b == null))
            enableSkill = EditorGUI.Toggle(rect, enableSkill);
        if (b != null && b.enable != enableSkill)
        {
            Undo.RegisterCompleteObjectUndo(targetObject, $"{targetObject.GetType().FullName} : Set skill eabled");
            b.enable = enableSkill;
            targetObject.ValidateSkillBehaviours();
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
forward:{contoller.transform.forward},
forward.z:{contoller.transform.forward.z},
IsTeleporting:{contoller.teleportable.IsTeleporting}
", MessageType.Info); //FallingTimeElapsed: { contoller.FallingTimeElapsed}
        }
        this.Iterate(OnDrawProperty, BeginDrawProperty);
    }
    void BeginDrawProperty()
    {
        using (new EditorGUI.DisabledGroupScope(true))
        {
            EditorGUILayout.Vector2Field("Velocity", targetObject.velocity);
        }
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