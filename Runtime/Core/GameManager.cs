using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using Yu5h1Lib.Game.Character;
using static GameObjectEx;

namespace Yu5h1Lib
{
    [RequireComponent(typeof(EventSystem), typeof(Canvas), typeof(InputSystemUIInputModule))]
    [RequireComponent(typeof(UI_Manager), typeof(SoundManager))]
    [DisallowMultipleComponent]
    public partial class GameManager : SingletonBehaviour<GameManager>
    {
        public static bool IsQuit = false;
        [RuntimeInitializeOnLoadMethod]
        static void RunOnStart() {
            Application.wantsToQuit -= Application_wantsToQuit;
            Application.wantsToQuit += Application_wantsToQuit;
            IsQuit = false;
            SceneController.RegistryLoadEvents();
        }
        private static bool Application_wantsToQuit()
        {
            IsQuit = true;
            GameManager.RemoveInstanceCache();
            //DG.Tweening.DOTween.Clear(true);
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
        public static bool IsSpeaking => 
            ui_Manager.Dialog_UI.gameObject.activeSelf ||
            ui_Manager.EndCredits.gameObject.activeSelf;

        public static bool IsBusy => IsGamePause || IsSpeaking;

        public static UnityAction<bool> OnPauseStateChanged;


        private string inputString;
        [ReadOnly]
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
        protected override void Init() {}

        public void Start()
        {
            Cursor.SetCursor(cursor, Vector2.zero, CursorMode.Auto);
            

            var player = GameObject.FindWithTag("Player");
            
            if (player)
            {
                cameraController.SetTarget(player.transform);
                if (player.TryGetComponent(out playerController))
                    playerController.host = Resources.Load<PlayerHost>(nameof(PlayerHost));

                PoolManager.instance.PrepareFromResourece<Transform>("Fx");
            }
        }
        void Update()
        {
            if (playerController)
                SoundManager.instance.audioListener.transform.position = playerController.transform.position;
            else
                SoundManager.instance.audioListener.transform.position = cameraController.transform.position;

            if (input.GetButtonDown("Cancel"))
                Cancel();
            if (!Input.GetKey(KeyCode.LeftControl) && input.TryGetScrollWheelDelta(out float delta)) {
                if (playerController)
                    cameraController.ZoomCamera(delta);
            }
            //foreach (char c in Input.inputString)
            //{
            //    if (c == '\b') 
            //    {
            //        if (inputString.Length > 0)
            //            inputString = inputString.Substring(0, inputString.Length - 1);
            //    }
            //    else if (c == '\n' || c == '\r') // 判斷是否按下回車鍵
            //    {
            //        Debug.Log("Final Input: " + inputString);
            //        inputString = ""; 
            //    }
            //    else
            //    {
            //        inputString += c; 
            //    }
            //}
        }
        public void Submit()
        {
        }
        public void Cancel()
        {
            if (SceneController.IsLevelScene || playerController)
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
        public void PlayAudio(AudioClip clip,float volume)
        {
            if (!clip)
                return;
            this.GetOrAdd(out AudioSource audio);
            audio.clip = clip;
            audio.volume = volume;
            audio.PlayOneShot(clip,volume);
            //audio.Play();
        }
        public void PlayAudio(AudioSource source)
        {
            if (!source)
                return;
            PlayAudio(source.clip, source.volume);
        }
        public void DelayPlayAudio(float delay, AudioSource clip) 
        {
            DelayAction(delay, clip, PlayAudio);
        }
        private IEnumerator DelayAction<T>(float delay, T t, UnityAction<T> action)
        { 
            yield return new WaitForSeconds(delay);
            action?.Invoke(t);
        }
        #endregion
        #region Scene stuffs...
        public void StartNewGame()
        {
            GameManager.instance.LoadScene(1);
            TeleportGate2D.GateStates.Clear();
            CheckPoint.Clear();
        }
        public void ReloadCurrentScene() => SceneController.ReloadCurrentScene();
        public void LoadScene(string SceneName) => SceneController.LoadScene(SceneName);
        public void LoadScene(int SceneIndex) => SceneController.LoadScene(SceneIndex);
        #endregion
        #region Static

        public static void MovePlayer(Vector3 pos)
        {
            instance.playerController.transform.position = pos;
            cameraController.Focus();
        }

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
