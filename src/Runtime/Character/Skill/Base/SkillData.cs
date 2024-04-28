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
        public virtual SkillBehaviour GetBehaviour(Controller2D controller)
            => SkillBehaviour.Constructor(this, controller);
        public override string ToString() => $"{GetType()}";
    }
    public abstract class SkillData<T> : SkillData where T : SkillBehaviour
    {
        public override System.Type GetBehaviourType() => typeof(T);
    }
}
