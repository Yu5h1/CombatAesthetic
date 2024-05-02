using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

using static GameObjectEx;

namespace Yu5h1Lib
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(UI_Manager), typeof(EventSystem), typeof(InputSystemUIInputModule))]
    [RequireComponent(typeof(AudioListener), typeof(AudioSource))]
    [DisallowMultipleComponent]
    public partial class GameManager : SingletonComponent<GameManager>
    {
        public static bool IsQuit = false;
        [RuntimeInitializeOnLoadMethod]
        static void RunOnStart() {
            Application.wantsToQuit += Application_wantsToQuit;
            IsQuit = false;
        }

        private static bool Application_wantsToQuit()
        {
            IsQuit = true;
            GameManager.RemoveInstanceCache();
            DG.Tweening.DOTween.Clear(true);
            return true;
        }
        public interface IDispatcher
        {
            static GameManager gameManager => instance;
            static UI_Manager uiManager => UIController;
            static CameraController cameraController => GameManager.cameraController;
            static InputSystemUIInputModule inputModule => InputModule;
            static BaseInput input => InputModule.input;
        }
        #region Components
        public RectTransform rectTransform => (RectTransform)transform;
        private EventSystem _eventSystem;
        public static EventSystem eventsystem => instance._eventSystem;
        private Canvas _canvas_overlay;
        public static Canvas canvas_overlay => instance._canvas_overlay;
        private UI_Manager _uiController;
        public static UI_Manager UIController => instance._uiController;
        private InputSystemUIInputModule _InputModule;
        public static InputSystemUIInputModule InputModule => instance._InputModule;
        public static BaseInput input => InputModule.input;  
        #endregion

        public static CameraController cameraController => CameraController.instance;

        public GameSetting Setting => Resources.Load<GameSetting>(nameof(Setting));

        void Awake()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
            TryGetComponent(out _eventSystem);
            TryGetComponent(out _canvas_overlay);
            TryGetComponent(out _uiController);
            TryGetComponent(out _InputModule);
            DontDestroyOnLoad(this);
            SceneController.BeginLoadSceneHandler += BeginLoadSceneAsync;
            SceneController.EndLoadSceneHandler += EndLoadSceneAsync;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            SceneManager.sceneLoaded += sceneLoaded;
        }
        #region load scene event
        private static void EndLoadSceneAsync()
        {
            UIController.Start();
            instance.Start();
        }
        private static void BeginLoadSceneAsync() {}

        #region build-in 
        public static void ClearTempSingletonCaches()
        {
            CameraController.RemoveInstanceCache();
            PoolManager.RemoveInstanceCache();
            DG.Tweening.DOTween.KillAll();
        }
        private static void OnSceneUnloaded(Scene scene)
        {
            ClearTempSingletonCaches();
        }
        private static void sceneLoaded(Scene scene, LoadSceneMode mode)
        {
        }
        #endregion
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
        public static void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
        }
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
