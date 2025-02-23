using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Yu5h1Lib.Game
{
    [System.Serializable]
    public class Records
    {
        #region static
        public static string Key => nameof(Records);
        private static Records cache = new Records();
        public static string json => PlayerPrefs.GetString(Key, "");

        private static List<Record> _Saves;
        public static List<Record> Saves
        {
            get
            {
                try
                {
                    if (_Saves == null)
                    {
                        _Saves = PlayerPrefs.HasKey(Key) ? JsonUtility.FromJson<Records>(json).Items :
                                new List<Record>(5);
                    }
                    return _Saves;
                }
                catch (System.Exception)
                { return _Saves = new List<Record>(); }
            }
        }
        public static int CurrentSaveSlot
        {
            get => PlayerPrefs.GetInt(nameof(CurrentSaveSlot), 0);
            set
            {
                if ("SelectedSaveSlot out of bounds.".printErrorIf(value < 0 || value >= 5)) return;
                PlayerPrefs.SetInt(nameof(CurrentSaveSlot), value);
                PlayerPrefs.Save();
            }
        }
        public static Record current => Saves[CurrentSaveSlot];
        public static bool Any() => Saves.Any();
        public static void Prepare(int slotIndex)
        {
            if (Saves.Count <= slotIndex)
            {
                for (int i = Saves.Count; i < (slotIndex + 1); i++)
                    Saves.Add(new Record());
            }
        }
        public static void Save(int slotIndex, int buildIndex, Vector3 position)
        {
            Prepare(slotIndex);
            Saves[slotIndex].buildIndex = buildIndex;
            Saves[slotIndex].position = position;
            Save();
        }
        public static void Save(int buildIndex,Vector3 position) => Save(CurrentSaveSlot, buildIndex,position);
        public static void Load()
        {
            _Saves = null;
        }

        #endregion

        public List<Record> Items;

        public static void Save()
        {
            cache.Items = _Saves;
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(cache));
            PlayerPrefs.Save();
        }
        public static void Clear()
        {
            PlayerPrefs.DeleteKey(Key);
        }
    }
    [System.Serializable]
    public class Record
    {
 
        public int buildIndex;
        public Vector3 position;

        public string path => SceneUtility.GetScenePathByBuildIndex(buildIndex);
        public string name => AssetPathInfo.GetName(path);
        public Scene scene => SceneManager.GetSceneByBuildIndex(buildIndex);

        public bool Load()
        {
            if (buildIndex < 0)
                return false;
            SceneController.startPosition = position.IsNaN() ? null : position;
            SceneController.LoadScene(buildIndex);
            return true;
        }
        public void Clear()
        {
            buildIndex = -1;
            position = new Vector3(float.NaN, float.NaN, float.NaN);
        }

        public override string ToString() => buildIndex > 0 ?
            $"A{buildIndex.ToString("000")}.{name} {(position.IsNaN() ? "" : $"{position}")}":
            "No record";
    }

}