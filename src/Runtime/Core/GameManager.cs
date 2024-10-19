using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using Yu5h1Lib.Game.Character;
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
            SceneController.RegistryLoadEvents();
        }
        private static bool Application_wantsToQuit()
        {
            IsQuit = true;
            GameManager.RemoveInstanceCache();
            DG.Tweening.DOTween.Clear(true);
            return true;
        }
        #region Components
        public RectTransform rectTransform => (RectTransform)transform;
        private EventSystem _eventSystem;
        public static EventSystem eventsystem => instance._eventSystem;
        private Canvas _canvas_overlay;
        public static Canvas canvas_overlay => instance._canvas_overlay;
        private UI_Manager _ui_manager;
        public static UI_Manager ui_Manager => instance._ui_manager;
        private InputSystemUIInputModule _InputModule;
        public static InputSystemUIInputModule InputModule => instance._InputModule;
        public static BaseInput input => InputModule.input;
        #endregion

        public static CameraController cameraController => CameraController.instance;

        public GameSetting Setting => Resources.Load<GameSetting>(nameof(Setting));
        public static bool IsGamePause 
        { 
            get => Time.timeScale == 0; 
            set 
            {
                if (IsGamePause == value)
                    return;
                Time.timeScale = value ? 0 : 1;
                OnPauseStateChanged?.Invoke(value);
            } 
        }
        //public static bool IsGameStart;
        public static bool IsSpeaking => 
            ui_Manager.Dialog_UI.gameObject.activeSelf ||
            ui_Manager.EndCredits.gameObject.activeSelf;

        public static UnityAction<bool> OnPauseStateChanged;

        public Controller2D playerController;


        public Texture2D cursor;

        void Awake()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
            TryGetComponent(out _eventSystem);
            TryGetComponent(out _canvas_overlay);
            TryGetComponent(out _ui_manager);
            TryGetComponent(out _InputModule);
            DontDestroyOnLoad(this);
            
        }
        public void Start()
        {
            Cursor.SetCursor(cursor, Vector2.zero, CursorMode.Auto);
            var player = GameObject.FindWithTag("Player");
            //Debug.Log($"GameManager start find player:{player}");
            if (player)
            {
                cameraController.SetTarget(player.transform, SceneController.IsLevelScene);
                if (player.TryGetComponent(out playerController))
                {
                    playerController.host = Resources.Load<PlayerHost>(nameof(PlayerHost));
                    if (SceneController.IsLevelScene && playerController is AnimatorController2D animatorController2D)
                        animatorController2D.SetCursorFromDrawSkill();
                }
            }
        }
        void Update()
        {
            if (input.GetButtonDown("Cancel"))
                Cancel();
            if (!Input.GetKey(KeyCode.LeftControl) && input.TryGetScrollWheelDelta(out float delta)) {
                if (SceneController.IsLevelScene)
                {
                    cameraController.ZoomCamera(delta);
                }
            }
            if (input.GetMouseButtonDown(0))
            {
                if (!CameraController.DoesNotExists)
                {
                    cameraController.PlayCursorEffect();
                }
            }
        }
        public void Submit()
        {
        }
        public void Cancel()
        {
            if (SceneController.IsLevelScene)
                ui_Manager.PauseGame(!ui_Manager.LevelSceneMenu.activeSelf);
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
        public void PlayAudio(AudioClip clip,float volume = 1)
        {
            if (!clip)
                return;
            this.GetOrAdd(out AudioSource audio);
            audio.clip = clip;
            audio.volume = volume;
            audio.Play();
        }
        public void PlayAudio(AudioSource source)
        {
            if (!source)
                return;
            PlayAudio(source.clip);
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
