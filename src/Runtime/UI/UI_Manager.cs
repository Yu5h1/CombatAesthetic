using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using static Yu5h1Lib.GameManager.IDispatcher;
using static SceneController;
using System.Diagnostics.Tracing;

[DisallowMultipleComponent]
public class UI_Manager : MonoBehaviour
{    
    private RectTransform rectTransform => gameManager.rectTransform;

    [SerializeField]
    public RectTransform _Loading;
    public RectTransform Loading => TryFindGetInstantiateFromResourecsIfNull_UI(nameof(Loading), ref _Loading);
    [SerializeField]
    private TweenImage_UI _Fadeboard_UI;
    public TweenImage_UI Fadeboard_UI => TryFindGetInstantiateFromResourecsIfNull_UI(nameof(Fadeboard_UI), ref _Fadeboard_UI);

    public GameObject _LevelSceneMenuObject;
    public Menu LevelSceneMenu { get; private set; }
    public GameObject _StartSceneMenuObject;
    public Menu StartSceneMenu { get; private set; }

    private LoadAsyncBehaviour[] loadAsyncBehaviours;
    private void Awake()
    {                
        LevelSceneMenu = new Menu(TryFindGetInstantiateFromResourecsIfNull_UI(nameof(LevelSceneMenu), ref _LevelSceneMenuObject));
        StartSceneMenu = new Menu(TryFindGetInstantiateFromResourecsIfNull_UI(nameof(StartSceneMenu), ref _StartSceneMenuObject));
        if (Loading)
            loadAsyncBehaviours = Loading.GetComponentsInChildren<LoadAsyncBehaviour>(true);
        if (!loadAsyncBehaviours.IsEmpty())
            LoadSceneAsyncHandler += OnLoadAsyncBehaviours;
    }
    public void Start()
    {
        // SiblingIndex 0
        if (Fadeboard_UI)
            Fadeboard_UI.gameObject.SetActive(false);
        if (IsStartScene)
        {
            LevelSceneMenu.Dismiss();
            StartSceneMenu.Engage();
        }
        else {
            StartSceneMenu.Dismiss();
            LevelSceneMenu.Engage(false);
        }
    }
    public void PauseGame(bool YesNo)
    {
        #region Fade 
        if (gameManager.Setting.UI.FadeTransition)
        {

            return;
        } 
        #endregion
        LevelSceneMenu.root.SetActive(YesNo);
        Time.timeScale = LevelSceneMenu.root.activeSelf ? 0 : 1;
    }
    private T TryFindGetInstantiateFromResourecsIfNull_UI<T>(string n, ref T result) where T : Object
    {
        if (result)
            return result;
        if (!transform.TryGetComponentInChildren(n, out result))
            result = GameObjectEx.InstantiateFromResourecs<T>($"UI/{n}", transform);
        (result switch
        {
            Component component => component.gameObject,
            GameObject obj => obj,
            _ => null
        })?.SetActive(false);
        return result;
    }
    private void OnLoadAsyncBehaviours(float percentage)
    {
        foreach (var item in loadAsyncBehaviours)
            item.OnProcessing(percentage);
    }
    public class Menu
    {
        public enum Type
        {
            Main,
            Level
            //...
        }
        public GameObject root { get; private set; }
        public RectTransform rectTransform => root.GetComponent<RectTransform>();
        private Button _Submit;
        public Button Submit => _Submit ?? (TryFindButton(nameof(Submit), out _Submit) ? _Submit : null);
        private Button _Cancel;
        public Button Cancel => _Cancel ?? (TryFindButton(nameof(Cancel), out _Cancel) ? _Cancel : null);

        public Menu(GameObject menu)
        {
            root = menu;
        }
        public void FadeIn(float duration)
        {
            if (!root.activeSelf)
                root.SetActive(true);
            var c = rectTransform.GetComponent<CanvasGroup>();
            c.alpha = 0;
            c.DOFade(1, duration);
        }
        public bool TryFindButton(string name, out Button button)
        {
            button = null;
            if (!root.transform.TryGetComponentInChildren(name, out button))
                throw new MissingReferenceException($"{root.name} name:{name} Botton could not be found !");
            return true;
        }
        public void Engage(bool activateRoot = true)
        {
            if (root.activeSelf != activateRoot)
                root.SetActive(activateRoot);
            if (Submit)
            {
                Submit.onClick.RemoveAllListeners();
                ((Text)Submit.targetGraphic).text = IsStartScene ? "Start" : "Restart";
                Submit.onClick.AddListener(OnSubmitClick);
            }
            if (Cancel)
            {
                Cancel.onClick.RemoveAllListeners();
                ((Text)Cancel.targetGraphic).text = IsStartScene ? "Exit" : "Main Menu";
                Cancel.onClick.AddListener(OnCancelClick);               
            }
        }
        public void Dismiss()
        {
            if (root.activeSelf)
                root.SetActive(false);
            if (Submit)
                Submit.onClick.RemoveAllListeners();
            if (Cancel)
                Cancel.onClick.RemoveAllListeners();
        }
        private static void OnSubmitClick()
        {
            if (IsStartScene)
                LoadScene(1);
            else
                ReloadCurrentScene();
        }
        private static void OnCancelClick()
        {  
            if (IsStartScene)
                gameManager.ExitGame();
            else
                gameManager.LoadScene(0);
        }
    }
}
