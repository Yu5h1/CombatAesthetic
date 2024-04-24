using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

using static GameObjectEx;

namespace Yu5h1Lib
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(UIManager), typeof(EventSystem), typeof(InputSystemUIInputModule))]
    [RequireComponent(typeof(AudioListener), typeof(AudioSource))]
    [DisallowMultipleComponent]
    public partial class GameManager : MonoBehaviour
    {
        public interface IDispatcher
        {
            static GameManager gameManager => instance;
            static UIManager uiManager => UIController;
            static CameraController cameraController => GameManager.cameraController;
            static InputSystemUIInputModule inputModule => InputModule;
            static BaseInput input => InputModule.input;
        }

        private static GameManager _instance;
        public static GameManager instance
            => FindOrInstantiateFromResourecsIfNull(nameof(GameManager), ref _instance);

        #region Components
        public RectTransform rectTransform => (RectTransform)transform;
        private EventSystem _eventSystem;
        public static EventSystem eventsystem => instance.GetOrAddIfNull(ref instance._eventSystem);
        private Canvas _canvas_overlay;
        public static Canvas canvas_overlay => instance.GetOrAddIfNull(ref instance._canvas_overlay);
        private UIManager _uiController;
        public static UIManager UIController => instance.GetOrAddIfNull(ref instance._uiController);
        private InputSystemUIInputModule _InputModule;
        public static InputSystemUIInputModule InputModule => instance.GetOrAddIfNull(ref instance._InputModule);
        public static BaseInput input => InputModule.input;

        #endregion

        public static CameraController cameraController => CameraController.instance;

        public GameSetting Setting => Resources.Load<GameSetting>(nameof(Setting));

        void Awake()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
            DontDestroyOnLoad(this);
            SceneController.BeginLoadSceneHandler += BeginLoadSceneAsync;
            SceneController.EndLoadSceneHandler += EndLoadSceneAsync;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            SceneManager.sceneLoaded += sceneLoaded;
        }

        private static void EndLoadSceneAsync()
        {
            UIController.Start();
            instance.Start();
        }
        private static void BeginLoadSceneAsync()
        {
            
        }

        #region build-in load scene event
        private static void OnSceneUnloaded(Scene scene)
        {
            CameraController.RemoveInstanceCache();
            PoolManager.RemoveInstanceCache();
            DG.Tweening.DOTween.KillAll();
        }
        private static void sceneLoaded(Scene scene, LoadSceneMode mode)
        {
        }
        #endregion
        public void Start()
        {            
            if (SceneController.IsLevelScene)
            {
                Debug.Log(PoolManager.canvas);

            }
        }
        void Update()
        {
            if (input.GetButtonDown("Cancel"))
            {
                if (SceneController.IsLevelScene)
                    UIController.PauseGame(!UIController.LevelSceneMenu.root.activeSelf);
            }
            if (!Input.GetKey(KeyCode.LeftControl) && input.TryGetScrollWheelDelta(out float delta)) {
                if (SceneController.IsLevelScene)
                {
                    cameraController.ZoomCamera(delta);
                }
            }
        }
        public void Despawn(GameObject obj, DespawnReason reason) => StatsManager.instance?.Despawn(obj, reason);
        #region FX
        public void PlayAudio(AudioSource source)
        {
            if (!source)
                return;
            this.GetOrAdd(out AudioSource audio);
            audio.clip = source.clip;
            audio.volume = source.volume;
            audio.Play();
        }
        #endregion
        #region Scene stuffs...
        public void ReloadCurrentScene() => SceneController.ReloadCurrentScene();
        public void LoadScene(string SceneName) => SceneController.LoadScene(SceneName);
        public void LoadScene(int SceneIndex) => SceneController.LoadScene(SceneIndex);
        public void ExitGame() => SceneController.ExitGame();
        #endregion

        //public void DetectMouseAttack()
        //{
        //    if (input.GetMouseButtonDown(0))
        //    {
        //        var hit = Physics2D.GetRayIntersection(cameraController.camera.ScreenPointToRay(input.mousePosition));
        //        if (hit)
        //        {
        //            if (hit.transform.root.TryGetComponent(out AttributeStatBehaviour hitresult))
        //                hitresult.Damage(30);
        //        }
        //    }
        //}
    }
}
