using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;


namespace Yu5h1Lib
{
    [DisallowMultipleComponent]
    public class CheckPoint : PlayerEvent2D
    {
        #region Save
        protected static string SaveKey => nameof(CheckPoint);
        [System.Serializable]
        public class Data
        {
            public int sceneIndex;
            public Vector3 position;
        }
        [System.Serializable]
        public class DataList
        {
            public List<Data> Items;
        }

        private static string cachedJsonData;
        public static List<Data> Saves
        {
            get
            {
                try
                {
                    if (cachedJsonData == null || !PlayerPrefs.HasKey(SaveKey))
                        cachedJsonData = PlayerPrefs.GetString(SaveKey, "");

                    var datas = JsonUtility.FromJson<DataList>(cachedJsonData);
                    return datas?.Items ?? new List<Data>();
                }
                catch (System.Exception)
                { return new List<Data>(); }
            }
            set
            {
                var saves = new DataList { Items = value };
                var json = JsonUtility.ToJson(saves);

                if (cachedJsonData == json)
                    return;

                cachedJsonData = json;
                PlayerPrefs.SetString(SaveKey, json);
                PlayerPrefs.Save();
            }
        }

        private const string SelectedSaveSlotKey = "SelectedSaveSlot";
        public static int SelectedSaveSlot
        {
            get => PlayerPrefs.GetInt(SelectedSaveSlotKey, 0); 
            set
            {
                if ("SelectedSaveSlot out of bounds.".printErrorIf(value < 0 || value >= 5)) return; 
                PlayerPrefs.SetInt(SelectedSaveSlotKey, value);
                PlayerPrefs.Save();
            }
        }
        #endregion
        public static void InitinalizeCheckPoints()
        {
            checkPoints = FindObjectsByType<CheckPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var item in checkPoints)
                item.PerformIsChecked();
        }
        private static CheckPoint[] checkPoints = new CheckPoint[0];

        internal static int sceneIndex;
        internal static Vector3? position;

        public static bool Exists => PlayerPrefs.HasKey(SaveKey);

        public float volume = 1;

        private SpriteRenderer spriteRenderer;

        public UnityEvent CheckedAction;
        public UnityEvent UncheckedAction;

        private void Reset() 
        {
            GetComponent<CircleCollider2D>().isTrigger = true;
        }
        private void Awake()
        {
            if (checkPoints.IsEmpty())
            {
                checkPoints = FindObjectsByType<CheckPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (var item in checkPoints)
                    item.PerformIsChecked();
            }
        }
        private void PerformIsChecked()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (!spriteRenderer)
                return;
            if (gameObject.scene.buildIndex == sceneIndex && transform.position == position)
                CheckedAction?.Invoke();
            else
                UncheckedAction?.Invoke();
        }
        private void OnTriggerEnter2D(Collider2D other)
        {            
            if (!Validate(other))
                return;
            if (sceneIndex == SceneController.ActiveSceneIndex && position == transform.position)
                return;

            var activeScene = SceneManager.GetActiveScene();
            if (activeScene.buildIndex > 0)
            {
                sceneIndex = activeScene.buildIndex;
                position = transform.position;
                Save();
            }
                        
            foreach (var c in checkPoints)
                c.PerformIsChecked();
        }
        public static void Save()
        {
            PlayerPrefs.SetString(SaveKey, $"{sceneIndex};{position}");
        }
        public static void Clear()
        {
            checkPoints = null;
            position = null;
            sceneIndex = 0;
            PlayerPrefs.DeleteKey(SaveKey);
        }
        public static bool Load()
        {
            if (!ParseDataFromPlayerPrefs())
                return false;
            SceneController.startPosition = position;
            SceneController.LoadScene(sceneIndex);
            return true;
        }
        private static bool ParseDataFromPlayerPrefs()
        {
            if (!Saves.IsEmpty())
                return false;
            var data = Saves[SelectedSaveSlot];
            position = data.position;
            sceneIndex = data.sceneIndex;
            return true;
        }
        public void PlayAudio(AudioClip clip)
        {
            GameManager.instance.PlayAudio(clip, volume);
        }
    }
}

