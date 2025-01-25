using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Yu5h1Lib;

namespace KFC
{
    public static class AnimatorStateEx
    {
        [MenuItem("CONTEXT/AnimatorState/Add Hurt Exit Transition")]
        public static void AddHurtExitTransition(MenuCommand command)
        {
            AnimatorState state = (AnimatorState)command.context;
            if (!state.transitions.TryGet(
                t => t.conditions.TryGet(c => c.parameter == "Hurt", out _), out AnimatorStateTransition t))
                t = state.AddExitTransition();
            t.name = "HurtOut";
            t.hasExitTime = false;
            t.duration = 0;
            t.AddCondition(AnimatorConditionMode.If, 0, "Hurt");

        }
        [MenuItem("CONTEXT/AnimatorStateTransition/Set Action Index by hash")]
        public static void SetSkillIndexbyhash(MenuCommand command)
        {
            var t = (AnimatorStateTransition)command.context;
            SetIndexbyhash(t, "IndexOfAction", "IndexOfSkill");
        }
        [MenuItem("CONTEXT/AnimatorTransitionBase/Set Action Index by hash")]
        public static void SetSkillIndexbyhashBase(MenuCommand command)
        {
            var t = (AnimatorTransitionBase)command.context;
            SetIndexbyhash(t, "IndexOfAction", "IndexOfSkill");
        }
        private static void SetIndexbyhash(AnimatorTransitionBase t,params string[] names)
        {
            var conditions = t.conditions;
            for (int i = 0; i < conditions.Length; i++)
            {
                var item = conditions[i];
                if (item.parameter.EqualsAny(names))
                {
                    item.mode = AnimatorConditionMode.Equals;
                    item.threshold = t.destinationState.nameHash;
                }
                conditions[i] = item;
            }
            t.conditions = conditions;
            EditorUtility.SetDirty(t);
        }
    }
}