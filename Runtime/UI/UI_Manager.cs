using UnityEngine;
using static SceneController;
using Yu5h1Lib;

[DisallowMultipleComponent]
public class UI_Manager : SingletonBehaviour<UI_Manager>
{
    [ReadOnly,SerializeField]
    private UI_Menu _currentMenu;
    public static UI_Menu currentMenu { get => instance._currentMenu; private set => instance._currentMenu = value; }

    [SerializeField]
    public RectTransform _Loading;
    public RectTransform Loading => Build(nameof(Loading), ref _Loading);
    //[SerializeField]
    //private TweenImage_UI _Fadeboard_UI;
    //public TweenImage_UI Fadeboard_UI => Build(nameof(Fadeboard_UI), ref _Fadeboard_UI);

    private UI_Menu _LevelSceneMenu;
    public UI_Menu LevelSceneMenu => Build(nameof(LevelSceneMenu), ref _LevelSceneMenu);
    private UI_Menu _StartSceneMenu;
    public UI_Menu StartSceneMenu => Build(nameof(StartSceneMenu), ref _StartSceneMenu);

    private UI_DialogBase _Dialog_UI;
    public UI_DialogBase Dialog_UI => Build(nameof(Dialog_UI), ref _Dialog_UI);

    private LoadAsyncBehaviour[] loadAsyncBehaviours;

    private UI_Attribute _PlayerAttribute_UI;
    public UI_Attribute PlayerAttribute_UI => Build(nameof(PlayerAttribute_UI), ref _PlayerAttribute_UI);

    private UI_DialogBase _EndCredits;
    public UI_DialogBase EndCredits => Build(nameof(EndCredits), ref _EndCredits);

    public UI_Statbar statbar_UI_Source => Resources.Load<UI_Statbar>($"UI/BaseStatBar_UI");
    public UI_Attribute attribute_UI_Source => Resources.Load<UI_Attribute>("UI/BaseAttribute_UI");


    protected override void Init()
    {

    }
    private void Awake()
    {
        if (Loading)
            loadAsyncBehaviours = Loading.GetComponentsInChildren<LoadAsyncBehaviour>(true);
        if (!loadAsyncBehaviours.IsEmpty())
        {
            LoadSceneAsyncHandler -= OnLoadAsyncBehaviours;
            LoadSceneAsyncHandler += OnLoadAsyncBehaviours;
        }
    }
    public void Start()
    {
        GameManager.eventsystem.SetSelectedGameObject(null);
        //if (Fadeboard_UI)
        //    Fadeboard_UI.gameObject.SetActive(false);
        if (IsStartScene)
        {
            //LevelSceneMenu.Dismiss();
            //    PlayerAttribute_UI.gameObject.SetActive(false);
            if (_LevelSceneMenu)
                GameObject.DestroyImmediate(_LevelSceneMenu.gameObject);
            if (_PlayerAttribute_UI)
                GameObject.DestroyImmediate(_PlayerAttribute_UI.gameObject);
            StartSceneMenu.Engage();
            
        }
        else if (IsLevelScene || GameManager.instance.playerController)
        {
            if (_StartSceneMenu)
                GameObject.DestroyImmediate(_StartSceneMenu.gameObject);
            //    StartSceneMenu.Dismiss(true);

            Dialog_UI.transform.SetAsLastSibling();
            var playerMenu = PlayerAttribute_UI.GetComponent<UI_Menu>();
  
            playerMenu.previous = LevelSceneMenu;
            LevelSceneMenu.previous = playerMenu;
            LevelSceneMenu.Dismiss(false);

            playerMenu?.Engage();
        }
}
    #region Events
    public void PointerClick()
    {

    }
    public void OnCancelPressed()
    {
        //if (SceneController.IsLevelScene || GameManager.instance.playerController)
        //    PauseGame(!LevelSceneMenu.activeSelf);
        currentMenu?.ReturnToPrevious();
    } 
    #endregion
    //public void PauseGame(bool YesNo)
    //{
    //    #region Fade 
    //    if (GameManager.instance.Setting.UI.FadeTransition)
    //    {
    //        return;
    //    } 
    //    #endregion
    //    LevelSceneMenu.gameObject.SetActive(YesNo);
    //    if (YesNo)
    //        LevelSceneMenu.canvasGroup.alpha = 1;
    //    GameManager.eventsystem.SetSelectedGameObject(null);
    //    GameManager.IsGamePause = YesNo;
    //}

    public static void Engage(UI_Menu menu)
    {
        menu.gameObject.SetActive(true);
        menu.transform.SetAsLastSibling();
        currentMenu = menu;
    }
    public static void Dismiss(UI_Menu menu)
    {
        menu.gameObject.SetActive(false);
        currentMenu = null;
    }


    /// <summary>
    /// close self after moving to next menu
    /// </summary>
    public void ChangeMenu(UI_Menu next, bool dismiss)
    {
        if (!next)
            return;
        if (!next.DisallowPreviouse && !next.previous)
            next.previous = currentMenu;
        if (dismiss && !next.transform.IsChildOf(transform))
            currentMenu.Dismiss();
        next.Engage();
    }
    public void SwitchMenu(UI_Menu menu) => ChangeMenu(menu, true);
    public void Popup(UI_Menu popupmenu) => ChangeMenu(popupmenu, false);

    public void ChangeMenu(string MenuName, bool close)
    {
        if (!transform.TryGetComponentInChildren(MenuName, out UI_Menu menu))
            return;
        ChangeMenu(menu, close);
    }
    public void SwitchMenu(string MenuName) => ChangeMenu(MenuName, true);
    public void Popup(string MenuName) => ChangeMenu(MenuName, false);


    /// <summary>
    /// TryFindGetInstantiateFromResourecsIfNull_UI
    /// </summary>
    private T Build<T>(string n, ref T result) where T : Object
    {
        if (result)
            return result;
        if (!this.TryGetComponentInChildren(n, out result))
        {
            result = GameObjectEx.InstantiateFromResourecs<T>($"UI/{n}", transform);
        }
        if (result)
        {
            (result switch
            {
                Component component => component.gameObject,
                GameObject obj => obj,
                _ => null
            }).SetActive(false);
        }
        else
            Debug.LogWarning($"{n} does not exists.");
        return result;
    }
    private void OnLoadAsyncBehaviours(float percentage)
    {
        foreach (var item in loadAsyncBehaviours)
            item.OnProcessing(percentage);
    }


}
