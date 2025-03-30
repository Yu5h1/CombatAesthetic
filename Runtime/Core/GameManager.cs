using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using Yu5h1Lib.Game.Character;

namespace Yu5h1Lib
{
    [RequireComponent(typeof(EventSystem), typeof(Canvas), typeof(InputSystemUIInputModule))]
    [RequireComponent(typeof(UI_Manager), typeof(SoundManager))]
    [DisallowMultipleComponent]
    public partial class GameManager : SingletonBehaviour<GameManager> , IGameManager
    {
        public static bool IsQuit = false;
        public static GameSetting Setting => GameSetting.instance;
        public static DebugSetting debugSetting => DebugSetting.instance;

        [RuntimeInitializeOnLoadMethod]
        private static void Initialize() {
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
        [SerializeField,ReadOnly]
        private EventSystem _eventSystem;
        public static EventSystem eventsystem => instance._eventSystem;
        [SerializeField, ReadOnly]
        private Canvas _canvas_overlay;
        public static Canvas canvas_overlay => instance._canvas_overlay;
        [SerializeField, ReadOnly]
        private UI_Manager _ui_manager;
        public static UI_Manager ui_Manager => instance._ui_manager;
        private InputSystemUIInputModule _InputModule;
        public static InputSystemUIInputModule InputModule => instance._InputModule;
        public static BaseInput input => InputModule.input;
        #endregion

        private bool _isPaused;
        public bool isPaused
        { 
            get => _isPaused;
            set 
            {
                if (isPaused == value)
                    return;
                _isPaused = value;
                Time.timeScale = value ? 0 : 1;
                //Physics2D.simulationMode = value ? SimulationMode2D.Script : SimulationMode2D.FixedUpdate;
                OnPauseStateChanged();
                _pauseStateChanged?.Invoke(isPaused);
            } 
        }

        public static bool IsGamePaused
        {
            get => instance.isPaused;
            set => instance.isPaused = value;
        }

        public static bool IsSpeaking() => UI_Manager.IsSpeaking();
        public static bool NotSpeaking() => !UI_Manager.IsSpeaking();

        public static bool IsBusy() => IsGamePaused || IsSpeaking() || CameraController.IsPerforming;

        [SerializeField]
        private UnityEvent<bool> _pauseStateChanged;
        public static event UnityAction<bool> PauseStateChanged
        { 
            add  => instance._pauseStateChanged.AddListener(value);
            remove => instance._pauseStateChanged.RemoveListener(value);
        }

        [SerializeField]
        private UnityEvent<CharacterController2D> _foundPlayer;
        public static event UnityAction<CharacterController2D> FoundPlayer
        {
            add => instance._foundPlayer.AddListener(value);
            remove => instance._foundPlayer.RemoveListener(value);
        }

        public static event UnityAction<CharacterController2D> overridePlayerFailed;

        [ReadOnly]
        public CharacterController2D playerController;
        [SerializeReference, ReadOnly]
        public CharacterController2D[] characters ;

        public CursorInfo[] cursors;



        public UnityEvent CancelPressed;

        [SerializeField,ReadOnly]
        private bool _IsReady;
        public static bool IsReady { get => instance._IsReady; private set => instance._IsReady = value; }

        protected override void OnInstantiated()
        {
            gameObject.name = gameObject.GetOriginalName();
        }
        
        protected override void Awake()
        {
            base.Awake();
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
            DontDestroyOnLoad(this);
        }

        protected override void OnInitializing() 
        {
            this.GetComponent(ref _eventSystem);
            this.GetComponent(ref _canvas_overlay);
            this.GetComponent(ref _ui_manager);
            this.GetComponent(ref _InputModule);
        }

        public void Start()
        {
            IsReady = false;

            Input.imeCompositionMode = IMECompositionMode.Off;
            Time.timeScale = 1;
            isPaused = false;

            foreach (var item in FindObjectsByType<PlayerInput>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            {
                item.uiInputModule = InputModule;
            }
            characters = GameObject.FindObjectsByType<CharacterController2D>(FindObjectsInactive.Include,FindObjectsSortMode.InstanceID);
            var player = GameObject.FindWithTag("Player");

            if (player)
            {
                if (player.transform.root != player.transform)
                    player = player.transform.root.gameObject;

                CameraController.instance.Target = player.transform;
                CameraController.instance.Focus(player.transform);
                if (player.TryGetComponent(out playerController))
                {
                    playerController.host = Resources.Load<PlayerHost>(nameof(PlayerHost));
                    playerController.attribute.StatDepleted -= PlayerHealthDepleted;
                    playerController.attribute.StatDepleted += PlayerHealthDepleted;
                }
                PoolManager.instance.PrepareFromResourece("Fx");
                _foundPlayer?.Invoke(player.GetComponent<CharacterController2D>());
            }
            else
                cursors[0].Use();

            //foreach (var agent in transform.GetComponentsInChildren<GameManagementAgent>())
            //    agent.GameStart?.Invoke();
            IsReady = true;
        }
        void Update()
        {
            if (playerController)
            {
                SoundManager.instance.audioListener.transform.Sync(playerController ?
                    playerController.transform : CameraController.instance.transform, true, false, false);
            }
             

            if (input.GetButtonDown("Cancel"))
                OnCancelPressed();

#if UNITY_EDITOR
            if (playerController && Input.GetKeyDown(KeyCode.F10))
            {
                playerController.Floatable = !playerController.Floatable;
            }
#endif
        }
        private void OnPauseStateChanged()
        {
            if (IsQuit || !IsReady )
                return;
            if (!characters.IsEmpty())
            {
                foreach (var c in characters)
                    c.PauseStateChange(IsGamePaused);
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
            Teleporter.GateStates.Clear();
            CheckPoint.Clear();
        }
        public void ReloadCurrentScene() => SceneController.ReloadCurrentScene();
        public void LoadScene(string SceneName) => SceneController.LoadScene(SceneName);
        public void LoadScene(int SceneIndex) => SceneController.LoadScene(SceneIndex);
        #endregion
        #region Static

        public static void MovePlayer(Vector2 pos,Quaternion? rot = null)
        {
            if (Teleporter.MoveCharacter(instance.playerController, pos, rot) && CameraController.instance.follow)
                CameraController.instance.Focus(instance.playerController.transform);
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
        private static void OnPlayerFailed(CharacterController2D player)
        {
            player.GetComponent<SpriteRenderer>().sortingLayerName = "Front";
            player.attribute.ui?.GetComponent<UI_Menu>()?.Dismiss();
            PoolManager.canvas.sortingLayerName = "Back";
            CameraController.instance.FoldUp("Back", 1);
            ui_Manager.LevelSceneMenu.GetComponent<MonoEventHandler>().enabled = false;
            ui_Manager.LevelSceneMenu.previous = null;
            ui_Manager.LevelSceneMenu.DisallowPreviouse = true;
            ui_Manager.LevelSceneMenu.Engage();
        }

        #region Methods
        public void EnablePlayerControl() => SetPlayerControllable(true);
        public void DisablePlayerControl() => SetPlayerControllable(false);
        public static void SetPlayerControllable(bool flag)
        {
            if (!instance.playerController)
                return;
            instance.playerController.controllable = flag;
        }

        #endregion

        #region Toggles
        public void TogglePlayerFly()
        {
            if (!playerController)
                return;
            playerController.Floatable = !playerController.Floatable;
        }
        public void ToggleGamePause() => IsGamePaused = !IsGamePaused;

        #endregion
    }    
    
    public interface IGameManager
    {
        void EnablePlayerControl();
        void DisablePlayerControl();
    }
}
