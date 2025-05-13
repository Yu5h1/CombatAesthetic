using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Yu5h1Lib;

public class SceneController : SingletonBehaviour<SceneController>
{
    public static Vector3? startPosition;
    public static Quaternion? startRotation;

    public static bool IsLevelScene => ActiveSceneIndex > 0;

    [ContextMenuItem("Reset", nameof(ResetDefaultStartPoint))]
    public Vector3 defaultStartPoint;
    public void ResetDefaultStartPoint() => defaultStartPoint = transform.position;


    [SerializeField]
    private UnityEvent _started;
    [SerializeField]
    private UnityEvent _loaded;

    protected override void OnInstantiated() {}
    protected override void OnInitializing() {}
    private void Start()
    {
        $"{GameManager.instance} is ready.".print();
        gameObject.layer = LayerMask.NameToLayer("Boundary");
        if (IsLevelScene || GameManager.instance.playerController)
        {
            CheckPoint.InitializeCheckPoints();
            CameraController.instance.PrepareSortingLayerSprites();
            Debug.Log($"{PoolManager.instance} was Created.\n{PoolManager.canvas}");
        }
    }
    private void OnAfterLoadSceneAsync()
    {
        if (startPosition == null)
            _started?.Invoke();
        else
        {
            GameManager.MovePlayer(startPosition.Value, startRotation);
            startPosition = null;
            _loaded?.Invoke();
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        // Avoiding OnTriggerExit2D triggered on EditorApplication.Exit
        if (GameManager.IsQuit || IsUnloading )//|| Teleporter.IsTeleporting(other.transform))
            return;
        if (other.TryGetComponent(out Teleportable teleportable) && teleportable.IsTeleporting)
            return;

        if (other.TryGetComponent(out AttributeBehaviour attributeBeahaviour))
            attributeBeahaviour.Affect(AttributeType.Health, AttributePropertyType.Current, AffectType.NEGATIVE, 100000000);
    }
    

    #region Methods

    public void log(string msg) => msg.print();

    [ContextMenu(nameof(Test))]
    public void Test()
    {
        if (Yu5h1Lib.Utility.SceneUtility.GetSceneOfDontDestroyOnLoad(out Scene scene))
            scene.name.print();
    }
    #endregion



    #region Static 


    public static event UnityAction BeginLoadSceneAsyncHandler;
    public static event UnityAction<float> LoadSceneAsyncHandler;
    public static event UnityAction AfterLoadSceneAsyncHandler;
    public static void ClearLoadAsyncEvent()
    {
        BeginLoadSceneAsyncHandler = null;
        LoadSceneAsyncHandler = null;
        AfterLoadSceneAsyncHandler = null;
    }
    public static void RegistryLoadEvents()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }
    public static Scene ActiveScene => SceneManager.GetActiveScene();
    public static int ActiveSceneIndex => ActiveScene.buildIndex;

    public enum StringSearchOption
    {
        Equals,
        StartsWith,
        Contains,
        EndsWith
    }
    public static bool IsSceneName(string name, StringSearchOption comparison = StringSearchOption.Equals) => comparison switch
    {
        StringSearchOption.StartsWith => ActiveScene.name.StartsWith(name),
        StringSearchOption.Contains => ActiveScene.name.Contains(name),
        StringSearchOption.EndsWith => ActiveScene.name.EndsWith(name),
        _ => ActiveScene.name.Equals(name)
    };


    public static bool IsStartScene => IsSceneName("Start");
    public static bool Isloading { get; private set; }
    /// <summary>
    /// There is a difference between IsLoading and IsUnloading. The unloading process ends faster than the loading process.
    /// </summary>
    public static bool IsUnloading;
    private static bool IsSceneUnLoaded() => !IsUnloading;
    public static void ReloadCurrentScene() => LoadScene(ActiveScene.buildIndex);
    public static void LoadScene(string SceneName) => LoadScene(SceneManager.GetSceneByName(SceneName).buildIndex);
    public static bool LoadScene(int SceneIndex) {

        if ($"Scene index [{SceneIndex}] is invalid. Build contains {SceneManager.sceneCountInBuildSettings} scenes.".
            printWarningIf(SceneIndex < 0 || SceneIndex >= SceneManager.sceneCountInBuildSettings))
            return false;

        IsUnloading = true;
        GameManager.instance.StartCoroutine(ref LoadSceneAsyncCoroutine, LoadSceneAsynchronously(SceneIndex));
        return true;
    }
    private static Coroutine LoadSceneAsyncCoroutine;
    private static IEnumerator LoadSceneAsynchronously(int SceneIndex)
    {
        Isloading = true;
#if UNITY_EDITOR
        UnityEditor.Selection.activeObject = null;
#endif             
        
        LoadSceneAsyncHandler?.Invoke(0);
        BeginLoadSceneAsync();
        yield return GameManager.ui_Manager.Loading.BeginLoad();        
        //Application.backgroundLoadingPriority = GameManager.storyManager.Play() ? ThreadPriority.Low ThreadPriority.Normal;

        var operation = SceneManager.LoadSceneAsync(SceneIndex,LoadSceneMode.Single);
        operation.allowSceneActivation = false;
        while (!operation.isDone)
        {
            LoadSceneAsyncHandler?.Invoke(Mathf.Clamp01(operation.progress / 0.9f) * 0.8f);

            if (operation.progress >= 0.9f)
            {
                if (GameManager.storyManager.ValidateLoadingStory())
                    yield return new WaitForSeconds(1);
                if (GameManager.storyManager.TryPlayLoadingStory())
                    yield return new WaitWhile(GameManager.storyManager.IsPerforming);
                operation.allowSceneActivation = true;
            }
            yield return null;
        }
        #region Manage resources

        var unloadOperation = Resources.UnloadUnusedAssets();
        while (!unloadOperation.isDone)
        {
            LoadSceneAsyncHandler?.Invoke(0.8f + (Mathf.Clamp01(operation.progress / 0.9f) * 0.1f));
            yield return null;
        }
        System.GC.Collect();

        var preloadables = Object.FindObjectsOfType<MonoBehaviour>().Where( b=>b is IPreloadable).Cast<IPreloadable>().ToArray();
        
        for (int i = 0; i < preloadables.Length; i++)
        {
            var progress = ((float)i ) / preloadables.Length;
            LoadSceneAsyncHandler?.Invoke(0.9f + (progress * 0.05f));
            yield return preloadables[i].Loading();
        }

        LoadSceneAsyncHandler?.Invoke(0.95f);

        #endregion


        LoadSceneAsyncHandler?.Invoke(0.99f);

        yield return new WaitUntil(IsSceneUnLoaded);
        AfterLoadSceneAsync();
        LoadSceneAsyncHandler?.Invoke(1.0f);
        yield return GameManager.ui_Manager.Loading.EndLoad();
        Isloading = false;

    }
    private static void BeginLoadSceneAsync()
    {
        BeginLoadSceneAsyncHandler?.Invoke();
        GameManager.instance.beginLoadScene?.Invoke();
    }
    private static void OnSceneUnloaded(Scene scene) => UnloadSingleton();
    public static void UnloadSingleton()
    {
        KeyCodeEventManager.RemoveInstanceCache();
        CameraController.RemoveInstanceCache();
        PoolManager.RemoveInstanceCache();
        RemoveInstanceCache();
        Isloading = IsUnloading = false;
    }
    private static void AfterLoadSceneAsync()
    {
        GameManager.instance.afterLoadScene?.Invoke();
        GameManager.instance.Start();
        GameManager.ui_Manager.Start();
        AudioManager.instance.Start();
        instance.OnAfterLoadSceneAsync();
        AfterLoadSceneAsyncHandler?.Invoke();
    }

    #endregion
}