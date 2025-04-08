using BoolFn = System.Func<bool>;
using Type = System.Type;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Yu5h1Lib;
using Yu5h1Lib.UI;
using Yu5h1Lib.Game;

[DisallowMultipleComponent]
public class UI_Manager : MonoBehaviour
{
    private static UI_Manager _instance;
    public static UI_Manager instance{ 
        get{
            if (_instance == null || !GameManager.IsQuit)
                _instance = GameManager.instance.GetComponent<UI_Manager>();
            return _instance;            
        }
    }

    [SerializeField, ReadOnly]
    private Canvas _canvas_overlay;
    public static Canvas canvas_overlay => instance._canvas_overlay;

    [ReadOnly,SerializeField]
    private UI_Menu _currentMenu;
    public static UI_Menu currentMenu { get => instance._currentMenu; private set => instance._currentMenu = value; }

    [SerializeField,ReadOnly]
    public LoadAsyncAgent _Loading;
    public LoadAsyncAgent Loading => Build(nameof(Loading), ref _Loading);
    public Image LoadingBackGround => Loading.GetComponent<Image>();


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

    [SerializeField]
    private UI_TextPerformance _textPerformance;
    public UI_TextPerformance textPerformance => _textPerformance;

    private static List<KeyValuePair<Type,BoolFn>> cancelActions = new List<KeyValuePair<Type, BoolFn>>();

    public static void AddCancelAction(Type type,BoolFn action)
    {
        int order = GetCancelActionOrder(type);
        if ($"The cancel action of {type} already exists.".printWarningIf(order >= 0))
            return;
        cancelActions.Add(new KeyValuePair<Type, BoolFn>(type, action));
        $"The cancel action of {type} was added".print();
    }
    public static int GetCancelActionOrder(Type type) => cancelActions.IndexOf(d => d.Key == type);
    public static void RemoveCancelAction(Type type) {
        int order = GetCancelActionOrder(type);
        if (order < 0)
            return;
        cancelActions.RemoveAt(order);    
    }
    public static void ClearCancelActions() => cancelActions.Clear();

    public static bool visible
    {
        get => canvas_overlay.enabled;
        set => canvas_overlay.enabled = value;
    }


    public static bool IsSpeaking() =>
       (instance._Dialog_UI?.gameObject?.activeInHierarchy == true) || (instance._EndCredits?.gameObject?.activeSelf == true);

    private void Awake()
    {
        if (Loading)
            loadAsyncBehaviours = Loading.GetComponentsInChildren<LoadAsyncBehaviour>(true);
        if (!loadAsyncBehaviours.IsEmpty())
        {
            SceneController.LoadSceneAsyncHandler -= OnLoadAsyncBehaviours;
            SceneController.LoadSceneAsyncHandler += OnLoadAsyncBehaviours;
        }
        SceneController.BeginLoadSceneAsyncHandler -= BeginLoadSceneAsync;
        SceneController.BeginLoadSceneAsyncHandler += BeginLoadSceneAsync;

        this.GetComponent(ref _canvas_overlay);

        //cancelActions.Clear();
        AddCancelAction(typeof(UI_DialogBase), () =>
        {
            if (IsSpeaking())
            {
                Dialog_UI.Skip();
                return true;
            }
            return false;
        });
    }

    private void BeginLoadSceneAsync()
    {
        textPerformance?.Stop();
    }

    private void OnLoadAsyncBehaviours(float percentage)
    {
        foreach (var item in loadAsyncBehaviours)
            item.OnProcessing(percentage);
    }
    public void Start()
    {
        GameManager.eventsystem.SetSelectedGameObject(null);

        if (EndCredits.transform.gameObject.activeSelf)
            EndCredits.gameObject.SetActive(false);

        if (SceneController.IsStartScene)
        {
            if (_LevelSceneMenu)
                GameObject.DestroyImmediate(_LevelSceneMenu.gameObject);
            if (_PlayerAttribute_UI)
                GameObject.DestroyImmediate(_PlayerAttribute_UI.gameObject);
            StartSceneMenu.Engage();

        }
        else if (SceneController.IsLevelScene || GameManager.instance.playerController)
        {
            if (_StartSceneMenu)
                GameObject.DestroyImmediate(_StartSceneMenu.gameObject);
            var playerMenu = PlayerAttribute_UI.GetComponent<UI_Menu>();

            playerMenu.previous = LevelSceneMenu;
            LevelSceneMenu.previous = playerMenu;
            LevelSceneMenu.Dismiss(false);
            playerMenu?.Engage();
        }
    }
    public void RemoveMenu()
    {
        if (_LevelSceneMenu)
            GameObject.Destroy(_LevelSceneMenu.gameObject);
        if (_PlayerAttribute_UI)
            GameObject.Destroy(_PlayerAttribute_UI.gameObject);
        if (_StartSceneMenu)
            GameObject.Destroy(_StartSceneMenu.gameObject);
    }
    #region Events
    private void OnRectTransformDimensionsChange()
    {
        AdjustAspectRatio();
    }
    void AdjustAspectRatio()
    {
        float targetAspect = 16f / 9f;
        float currentAspect = (float)Screen.width / Screen.height;

        if (Mathf.Abs(currentAspect - (16f / 10f)) < Mathf.Abs(currentAspect - targetAspect))
        {
            targetAspect = 16f / 10f;
        }

        int newWidth = Screen.width;
        int newHeight = (int)(newWidth / targetAspect);

        if (newHeight > Screen.height)
        {
            newHeight = Screen.height;
            newWidth = (int)(newHeight * targetAspect);
        }

        Screen.SetResolution(newWidth, newHeight, Screen.fullScreen);
    }

    public void PointerClick()
    {

    }
    public void OnCancelPressed()
    {
       
        if (cancelActions.Any(c => c.Value?.Invoke() == true))
            return;
        //if (StoryPerformance.current && !StoryPerformance.current.IsCompleted)
        //{
        //    StoryPerformance.current.MarkAsCompleted();
        //    return;
        //}
        currentMenu?.ReturnToPrevious();
    }
    #endregion

    public static void Engage(UI_Menu menu)
    {
        instance.Loading.transform.SetAsLastSibling();
        menu.MoveToLast();
        menu.gameObject.SetActive(true);
        currentMenu = menu;
    }
    public static void MoveToLast(Transform t)
    {
        if (t.parent == instance.Loading.transform.parent)
            t.SetSiblingIndex(instance.Loading.transform.GetSiblingIndex() - 1);
        else
            t.SetAsLastSibling();
    }
    public static void Dismiss(UI_Menu menu)
    {
        menu.gameObject.SetActive(false);
        currentMenu = null;
    }


    /// <summary>
    /// close self after moving to next menu
    /// </summary>
    public static void ChangeMenu(UI_Menu current,UI_Menu next, bool dismiss)
    {
        var parent = current ? current.transform : instance.transform;
        if (!next)
            return;
        if (!next.DisallowPreviouse && !next.previous)
            next.previous = currentMenu;
        if (dismiss && !next.transform.IsChildOf(parent))
            currentMenu.Dismiss();
        next.Engage();
    }
    public static void SwitchMenu(UI_Menu current, UI_Menu next) => ChangeMenu(current, next, true);
    public static void Popup(UI_Menu current, UI_Menu next) => ChangeMenu(current,next, false);

    public void SwitchMenu(UI_Menu menu) => ChangeMenu(null,menu, true);
    public void Popup(UI_Menu popupmenu) => ChangeMenu(null, popupmenu, false);

    public void ChangeMenu(string MenuName, bool close)
    {        
        if (transform.TryGetComponentInChildren(MenuName, out UI_Menu menu))
            ChangeMenu(null,menu, close);
    }
    public void SwitchMenu(string MenuName) => ChangeMenu(MenuName, true);
    public void Popup(string MenuName) => ChangeMenu(MenuName, false);

    public static void ActiveIfHasAnyRecords(Selectable control)
    {
        if (!control)
            return;
        control.gameObject.SetActive(Records.Any());
    }
    public static void SetInteractableIfHasAnyRecords(Selectable control)
    {
        if (!control)
            return;
        control.interactable = Records.Any();
    }


    /// <summary>
    /// TryFindGetInstantiateFromResourecsIfNull_UI
    /// </summary>
    private T Build<T>(string n, ref T result) where T : Object
    {
        if (result)
            return result;

        if (!this.TryGetComponentInChildren(n, out result))
            $"{n} not found in Reousource folder.".printErrorIf(!ResourcesUtility.TryInstantiateFromResources(out result, $"UI/{n}", transform, true));

        if (result)
        {
            var gobj = result switch
            {
                Component component => component.gameObject,
                GameObject obj => obj,
                _ => null
            };           
            gobj.SetActive(false);
            if (Loading)
                gobj.transform.SetSiblingIndex(Loading.transform.GetSiblingIndex()-1);
        }
        else
            Debug.LogWarning($"{n} does not exists.");
        return result;
    }


    public void ToggleActive(GameObject obj) => obj.SetActive(!obj.activeSelf);
    public void ToggleEnabled(Behaviour b) => b.enabled = !b.enabled;

    public void Prompt(string[] lines)
    {
        Dialog_UI.lines = lines;        
        Dialog_UI.gameObject.SetActive(true);
    }
    public void Prompt(string content) => Prompt(content.Split('\n'));

}
