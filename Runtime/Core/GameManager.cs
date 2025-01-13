using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
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
        public static GameSetting Setting => GameSetting.instance;
        public static DebugSetting debugSetting => DebugSetting.instance;
        public static bool DebugMode
        {
            get => debugSetting.enable;
            set => debugSetting.enable = value;
        }
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
        public static bool IsSpeaking => UI_Manager.IsSpeaking;

        public static bool IsBusy => IsGamePause || IsSpeaking;

        public static event UnityAction<bool> OnPauseStateChanged;
        public static event UnityAction OnFoundPlayer;
        public static event UnityAction<Controller2D> overridePlayerFailed;

        [ReadOnly]
        public Controller2D playerController;
        public Texture2D cursor;

        public UnityEvent CancelPressed;
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
            var player = GameObject.FindWithTag("Player");
            
            if (player)
            {
                if (player.transform.root != player.transform)
                    player = player.transform.root.gameObject;

                cameraController.SetTarget(player.transform);
                if (player.TryGetComponent(out playerController))
                {
                    playerController.host = Resources.Load<PlayerHost>(nameof(PlayerHost));
                    playerController.attribute.StatDepleted -= PlayerHealthDepleted;
                    playerController.attribute.StatDepleted += PlayerHealthDepleted;
                }
                PoolManager.instance.PrepareFromResourece("Fx");
                OnFoundPlayer?.Invoke();
            }else
                Cursor.SetCursor(cursor, Vector2.zero, CursorMode.Auto);
        }
        void Update()
        {
            SoundManager.instance.audioListener.transform.Sync(playerController ? 
                playerController.transform : cameraController.transform, true, false, false);

            if (input.GetButtonDown("Cancel"))
                OnCancelPressed();
            if (!Input.GetKey(KeyCode.LeftControl) && input.TryGetScrollWheelDelta(out float delta)) 
            {
                if (playerController)
                    cameraController.ZoomCamera(delta);
            }
        }
        public void OnSubmitPressed()
        {
        }
        public void OnCancelPressed()
        {
            CancelPressed?.Invoke();
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

        public static bool IsMovingPlayer { get; private set; }
        public static void MovePlayer(Vector3 pos)
        {
            IsMovingPlayer = true;
            instance.playerController.rigidbody.simulated = false;
            instance.playerController.transform.position = pos;
            instance.playerController.rigidbody.simulated = true;
            cameraController.Focus();
            IsMovingPlayer = false; 
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

        private void PlayerHealthDepleted(AttributeType flag)
        {
            if (flag.HasFlag(AttributeType.Health))
                (overridePlayerFailed ?? OnPlayerFailed).Invoke(playerController);
        }
        private static void OnPlayerFailed(Controller2D player)
        {
            player.GetComponent<SpriteRenderer>().sortingLayerName = "Front";
            player.attribute.ui?.GetComponent<UI_Menu>()?.Dismiss();
            PoolManager.canvas.sortingLayerName = "Back";
            CameraController.instance.FoldUp("Back", 1);
            GameManager.ui_Manager.LevelSceneMenu.GetComponent<MonoEventHandler>().enabled = false;
            GameManager.ui_Manager.LevelSceneMenu.previous = null;
            GameManager.ui_Manager.LevelSceneMenu.DisallowPreviouse = true;
            GameManager.ui_Manager.LevelSceneMenu.Engage();
        }
    }    
}
