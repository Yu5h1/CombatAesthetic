using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using System.Linq;

public static class AnimatorStateEx
{
    [MenuItem("CONTEXT/AnimatorState/Add Exit Transition")]
    public static void AddExitTransition(MenuCommand command)
    {
        AnimatorState state = (AnimatorState)command.context;
        var t = state.AddExitTransition();
        t.exitTime = 1;
        t.duration = 0;
        t.hasExitTime = true;
    }
    [MenuItem("CONTEXT/AnimatorStateTransition/Set Skill Index by hash")]
    public static void SetSkillIndexbyhash(MenuCommand command)
    {
        var t = (AnimatorStateTransition)command.context;
        var conditions = t.conditions;
        for (int i = 0; i < conditions.Length; i++)
        {
            var item = conditions[i];
            if (item.parameter == "IndexOfSkill")
            {
                item.mode = AnimatorConditionMode.Equals;
                item.threshold = t.destinationState.nameHash;
            }
            conditions[i] = item;
        }
        t.conditions = conditions;
        EditorUtility.SetDirty(t);
    }
    [MenuItem("CONTEXT/AnimatorStateTransition/ZeroDuration")]
    public static void ZeroDuration(MenuCommand command)
    {
        var t = (AnimatorStateTransition)command.context;
        t.duration = 0;
    }
    [MenuItem("CONTEXT/AnimatorTransitionBase/Set Skill Index by hash")]
    public static void SetSkillIndexbyhashBase(MenuCommand command)
    {
        var t = (AnimatorTransitionBase)command.context;
        var conditions = t.conditions;
        for (int i = 0; i < conditions.Length; i++)
        {
            var item = conditions[i];
            if (item.parameter == "IndexOfSkill")
            {
                item.mode = AnimatorConditionMode.Equals;
                item.threshold = t.destinationState.nameHash;
            }
            conditions[i] = item;
        }
        t.conditions = conditions;
        EditorUtility.SetDirty(t);
    }

    [MenuItem("CONTEXT/AnimatorState/Add Hurt Exit Transition")]
    public static void AddHurtExitTransition(MenuCommand command)
    {
        AnimatorState state = (AnimatorState)command.context;
if (!state.transitions.TryFind(
    t => t.conditions.TryFind(c => c.parameter == "Hurt", out _),out AnimatorStateTransition t))
            t = state.AddExitTransition();
        t.name = "HurtOut";
        t.hasExitTime = false;
        t.duration = 0;
        t.AddCondition(AnimatorConditionMode.If,0,"Hurt");
        
    }
}
