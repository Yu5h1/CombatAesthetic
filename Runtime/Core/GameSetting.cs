using SerializableAttribute = System.SerializableAttribute;
using UnityEngine;

namespace Yu5h1Lib
{
    public class GameSetting : ScriptableObject
    {
        protected static GameSetting _instance;
        public static GameSetting instance => ResourcesUtility.LoadAsInstance(ref _instance);
        public float characterGravityScale = 0.033333f;
    }
    public class GameSetting<T> : GameSetting where T : GameSetting
    {
        public new static T instance { 
            get {
                if (_instance is T instanceT)
                    return instanceT;
                T _instanceT = null;
                ResourcesUtility.LoadAsInstance(ref _instanceT);
                _instance = _instanceT;
                return _instanceT;
            }
        } 
    }
}
