using SerializableAttribute = System.SerializableAttribute;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

namespace Yu5h1Lib
{
    public class GameSetting : ScriptableObject
    {
        public static string filePath => AssetPathInfo.Combine(Application.persistentDataPath, $"{nameof(GameSetting)}.json");
        protected static GameSetting _instance;
        public static GameSetting instance => _instance = Resources.Load<GameSetting>(nameof(GameSetting));


        public static void Load() 
        {
            if ($"No saved data found at {filePath}".printWarningIf(!File.Exists(filePath)))
                return;
            if ("GameSetting does not exist in resources folder".printWarningIf(!instance))
                return;
            JsonUtility.FromJsonOverwrite(File.ReadAllText(filePath), instance);
            _instance.OnLoadFromJson();
            $"GameSetting loaded from: {filePath}".print();
        }
        protected virtual void OnLoadFromJson() {}
        public void Save()
        {
            File.WriteAllText(filePath,
                JsonConvert.SerializeObject(this));
        }
    }
    public class GameSetting<T> : GameSetting where T : GameSetting
    {
        public new static T instance => _instance as T;
    }
}
