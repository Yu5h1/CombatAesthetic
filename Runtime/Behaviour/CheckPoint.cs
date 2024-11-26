using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;


namespace Yu5h1Lib
{
    [DisallowMultipleComponent]
    public class CheckPoint : PlayerEvent2D
    {
        public static void InitinalizeCheckPoints()
        {
            checkPoints = FindObjectsByType<CheckPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var item in checkPoints)
                item.PerformIsChecked();
        }
        private static CheckPoint[] checkPoints = new CheckPoint[0];

        internal static int sceneIndex;
        internal static Vector3? position;

        public static bool Exists => PlayerPrefs.HasKey(nameof(CheckPoint));

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
            PlayerPrefs.SetString(nameof(CheckPoint), $"{sceneIndex};{position}");
        }
        public static void Clear()
        {
            checkPoints = null;
            position = null;
            sceneIndex = 0;
            PlayerPrefs.DeleteKey(nameof(CheckPoint));
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
            if (!PlayerPrefs.HasKey(nameof(CheckPoint)))
                return false;
            var content = PlayerPrefs.GetString(nameof(CheckPoint));        
            if (content.IsEmpty())
                return false;
            var items = content.Split(';');
            if (items.Length < 1)
                return false;
            if (!int.TryParse(items[0], out int index))
                return false;
            var floats = items[1].Trim('(', ')').Split(",");
            if (floats.Length < 2)
                return false;
            position = new Vector3(float.Parse(floats[0]), float.Parse(floats[1]), float.Parse(floats[2]));
            sceneIndex = index;
            return true;
        }
        public void PlayAudio(AudioClip clip)
        {
            GameManager.instance.PlayAudio(clip, volume);
        }
    }
}

