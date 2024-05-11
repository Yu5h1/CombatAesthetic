using UnityEngine;
using static SceneController;
using Yu5h1Lib;

[DisallowMultipleComponent]
public class UI_Manager : UI_Behaviour
{    
    [SerializeField]
    public RectTransform _Loading;
    public RectTransform Loading => Build(nameof(Loading), ref _Loading);
    [SerializeField]
    private TweenImage_UI _Fadeboard_UI;
    public TweenImage_UI Fadeboard_UI => Build(nameof(Fadeboard_UI), ref _Fadeboard_UI);

    public UI_Menu _LevelSceneMenu;
    public UI_Menu LevelSceneMenu => Build(nameof(LevelSceneMenu), ref _LevelSceneMenu);
    public UI_Menu _StartSceneMenu;
    public UI_Menu StartSceneMenu => Build(nameof(StartSceneMenu), ref _StartSceneMenu);

    public UI_DialogBase _Dialog_UI;
    public UI_DialogBase Dialog_UI => Build(nameof(Dialog_UI), ref _Dialog_UI);

    private LoadAsyncBehaviour[] loadAsyncBehaviours;

    public UI_Attribute _PlayerAttribute;
    public UI_Attribute PlayerAttribute_UI => Build(nameof(PlayerAttribute_UI), ref _PlayerAttribute);

    public UI_Statbar statbar_UI_Source => Resources.Load<UI_Statbar>($"UI/BaseStatBar_UI");
    public UI_Attribute attribute_UI_Source => Resources.Load<UI_Attribute>("UI/BaseAttribute_UI");

    public ParticleSystem pointerClick_UI_Fx;
    private void Awake()
    {
        if (Loading)
            loadAsyncBehaviours = Loading.GetComponentsInChildren<LoadAsyncBehaviour>(true);
        if (!loadAsyncBehaviours.IsEmpty())
            LoadSceneAsyncHandler += OnLoadAsyncBehaviours;
    }
    public void Start()
    {
        if (Fadeboard_UI)
            Fadeboard_UI.gameObject.SetActive(false);
        if (IsStartScene)
        {
            LevelSceneMenu.Dismiss();
            StartSceneMenu.Engage();
            
            PlayerAttribute_UI.gameObject.SetActive(false);
        }
        else if (IsLevelScene) {
            Dialog_UI.rectTransform.SetSiblingIndex(transform.childCount-1);
            StartSceneMenu.Dismiss();
            LevelSceneMenu.Engage(false);
            PlayerAttribute_UI.transform.SetSiblingIndex(0);
            PlayerAttribute_UI.FadeIn();
            if (GameManager.instance.playerController)
                GameManager.instance.playerController.attribute.ui = PlayerAttribute_UI;
        }
    }
    public void PointerClick()
    {
        if (!pointerClick_UI_Fx)
            return;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform,
            Input.mousePosition, null, out Vector2 localPoint)) 
        {
            pointerClick_UI_Fx.transform.position = localPoint;
            pointerClick_UI_Fx.Play();
        }
    }
    public void PauseGame(bool YesNo)
    {
        #region Fade 
        if (GameManager.instance.Setting.UI.FadeTransition)
        {
            return;
        } 
        #endregion
        LevelSceneMenu.gameObject.SetActive(YesNo);
        if (YesNo)
            LevelSceneMenu.canvasGroup.alpha = 1;
        GameManager.IsGamePause = YesNo;
    }
    /// <summary>
    /// TryFindGetInstantiateFromResourecsIfNull_UI
    /// </summary>
    private T Build<T>(string n, ref T result) where T : Object
    {
        if (result)
            return result;
        if (!this.TryGetComponentInChildren(n, out result))
            result = GameObjectEx.InstantiateFromResourecs<T>($"UI/{n}", transform);
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
