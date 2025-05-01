using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;

namespace Yu5h1Lib.Game.Character
{
    public class SkillData : ScriptableObject
    {
        [Tooltip("If spell is empty, the skill will be set as index skill option")]
        public string incantation;
        public float distance = 1;
        [Range(0,180)]
        public float angle = 180;

        public EnergyInfo[] costs;
        public virtual bool parallelizable => false;
        private Dictionary<string, int> _preCalculatedCost;
        public Dictionary<string, int> preCalculatedCost
        {
            get
            {
                if (_preCalculatedCost != null)
                    return _preCalculatedCost;
                _preCalculatedCost = costs.SelectMany(i =>
                i.attributeType.SeparateFlags()).GroupBy(d => d).Select(g => g.First()).ToDictionary(
                    t => $"{t}", t => costs.Sum(t));
                return _preCalculatedCost;
            }
        }
        public virtual System.Type GetBehaviourType() => typeof(SkillBehaviour);
        public virtual void CreateBehaviour(AnimatorCharacterController2D controller,ref SkillBehaviour behaviour)
            => SkillBehaviour.Constructor(this, controller,ref behaviour);
        public override string ToString() => $"{GetType()}";
    }
    public abstract class SkillData<T> : SkillData where T : SkillBehaviour
    {
        public override System.Type GetBehaviourType() => typeof(T);
    }
}
