using System.Linq;
using System.Reflection;
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
            t.SetIndexbyhash("IndexOfAction", "IndexOfSkill");
        }
        [MenuItem("CONTEXT/AnimatorTransitionBase/Set Action Index by hash")]
        public static void SetSkillIndexbyhashBase(MenuCommand command)
        {
            var t = (AnimatorTransitionBase)command.context;
            SetIndexbyhash(t, "IndexOfAction", "IndexOfSkill");
        }
        private static void SetIndexbyhash(this AnimatorTransitionBase t,params string[] names)
        {
            var conditions = t.conditions;
            bool added = false;
            for (int i = 0; i < conditions.Length; i++)
            {
                var item = conditions[i];
                if (item.parameter.EqualsAny(names))
                {
                    item.mode = AnimatorConditionMode.Equals;
                    item.threshold = t.destinationState.nameHash;
                    added = true;
                }
                conditions[i] = item;
            }            
            t.conditions = conditions;

            
            if (!added)
            {
                //Animator animator = Selection.activeGameObject.GetComponent<Animator>();
                //var index = animator.parameters.IndexOf(p => p.name.EqualsAny());
                //if (index > -1)
                //{
                //    t.AddCondition(AnimatorConditionMode.Equals, t.destinationState.nameHash, animator.parameters[index].name);
                //}
            }
            EditorUtility.SetDirty(t);
        }

        #region Clone
        [MenuItem("CONTEXT/AnimatorState/CloneComplete")]
        public static void CloneComplete(MenuCommand command)
        {
            AnimatorState originalState = (AnimatorState)command.context;
            AnimatorStateMachine stateMachine = originalState.GetParentStateMachine();

            if (stateMachine == null)
            {
                Debug.LogError("找不到 State 所屬的 StateMachine");
                return;
            }
            
            if (!originalState.TryGetChildInfo(out ChildAnimatorState info))
            {
                Debug.LogError("找不到 State 資訊");
                return;
            }

            AnimatorState newState = stateMachine.AddState(originalState.name + "_Copy", info.position + new Vector3(250, 0));
            newState.motion = originalState.motion;
            newState.behaviours = CloneBehaviours(originalState.behaviours, newState);

            newState.speed = originalState.speed;
            newState.writeDefaultValues = originalState.writeDefaultValues;
            newState.cycleOffset = originalState.cycleOffset;
            newState.mirror = originalState.mirror;
            newState.iKOnFeet = originalState.iKOnFeet;
            newState.tag = originalState.tag;

            // 複製 Transitions
            foreach (var trans in originalState.transitions)
            {
                AnimatorStateTransition newTrans = trans.destinationState == null ?
                    newState.AddExitTransition(false) :  newState.AddTransition(trans.destinationState);
                newTrans.name = trans.name;
                newTrans.duration = trans.duration;
                newTrans.exitTime = trans.exitTime;
                newTrans.hasExitTime = trans.hasExitTime;
                newTrans.hasFixedDuration = trans.hasFixedDuration;
                newTrans.interruptionSource = trans.interruptionSource;
                newTrans.offset = trans.offset;
                newTrans.orderedInterruption = trans.orderedInterruption;

                // 複製 Conditions（名稱不用異化）
                foreach (var cond in trans.conditions)
                {
                    newTrans.AddCondition(cond.mode, cond.threshold, cond.parameter);
                }
            }

            Debug.Log($"成功複製 Animator State：{originalState.name} -> {newState.name}");
        }
        public static bool TryGetChildInfo(this AnimatorState state,out ChildAnimatorState info)
        {
            info = default;
            foreach (var child in state.GetParentStateMachine().states)
            {
                if (child.state == state)
                {
                    info = child;
                    return true;
                }
            }
            return false;
        }
        private static AnimatorStateMachine GetParentStateMachine(this AnimatorState state)
        {
            AnimatorController ac = AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GetAssetPath(state));
            if (ac == null) return null;

            foreach (var layer in ac.layers)
            {
                AnimatorStateMachine sm = layer.stateMachine;
                AnimatorStateMachine found = FindStateMachineContainingState(sm, state);
                if (found != null) return found;
            }

            return null;
        }

        private static AnimatorStateMachine FindStateMachineContainingState(AnimatorStateMachine sm, AnimatorState target)
        {
            foreach (var s in sm.states)
            {
                if (s.state == target) return sm;
            }

            foreach (var child in sm.stateMachines)
            {
                AnimatorStateMachine found = FindStateMachineContainingState(child.stateMachine, target);
                if (found != null) return found;
            }

            return null;
        }

        private static StateMachineBehaviour[] CloneBehaviours(StateMachineBehaviour[] originalBehaviours, AnimatorState newState)
        {
            StateMachineBehaviour[] newBehaviours = new StateMachineBehaviour[originalBehaviours.Length];
            for (int i = 0; i < originalBehaviours.Length; i++)
            {
                StateMachineBehaviour original = originalBehaviours[i];
                StateMachineBehaviour clone = Object.Instantiate(original);
                newBehaviours[i] = clone;

                Undo.RecordObject(newState, "Add SMB");
                newState.AddStateMachineBehaviour(clone.GetType());
                CopyBehaviourValues(clone, newState.behaviours[i]);
            }
            return newBehaviours;
        }

        private static void CopyBehaviourValues(StateMachineBehaviour source, StateMachineBehaviour destination)
        {
            var fields = source.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields)
            {
                if (field.IsDefined(typeof(SerializeField), true) || field.IsPublic)
                {
                    field.SetValue(destination, field.GetValue(source));
                }
            }
        }
        #endregion
    }
}