using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Yu5h1Lib;
using Yu5h1Lib.Game.Character;

public class SceneController : SingletonBehaviour<SceneController>
{
    public static Vector3? startPosition;
    public static Quaternion? startRotation;

    public bool IsLevel;
    public string[] StartLines;
    public bool NoTalking;

    [ContextMenuItem("Reset", nameof(ResetDefaultStartPoint))]
    public Vector3 defaultStartPoint;
    public void ResetDefaultStartPoint() => defaultStartPoint = transform.position;

    #region Scene Preset
    void Reset() {}
    protected override void Init() {}
    #endregion
    private void Start()
    {
        $"{GameManager.instance} is ready.".print();
        Time.timeScale = 1;
        gameObject.layer = LayerMask.NameToLayer("Boundary");

        if (SceneController.IsLevelScene || GameManager.instance.playerController)
        {
            CheckPoint.InitinalizeCheckPoints();
            if (startPosition == null && !StartLines.IsEmpty() && !NoTalking)
            {
                GameManager.IsGamePause = true;
                GameManager.ui_Manager.Dialog_UI.lines = StartLines;                
                GameManager.ui_Manager.Dialog_UI.gameObject.SetActive(true);
            }
            CameraController.instance.PrepareSortingLayerSprites();
            Debug.Log($"{PoolManager.instance} was Created.\n{PoolManager.canvas}");
        }
    }
    private void OnAfterLoadSceneAsync() {
        if (ActiveSceneIndex > 0 && startPosition != null)
        {
            GameManager.MovePlayer(startPosition.Value,startRotation);
            startPosition = null;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Avoiding OnTriggerExit2D triggered on EditorApplication.Exit
        if (GameManager.IsQuit || IsSceneTransitioning || GameManager.IsMovingPlayer)
            return;
        if (other.TryGetComponent(out AttributeBehaviour attributeBeahaviour))
            attributeBeahaviour.Affect(AttributeType.Health, AffectType.NEGATIVE, 100000000);
    }
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
    public static bool IsLevelScene => instance.IsLevel;

    public static bool IsSceneTransitioning;
    private static bool IsSceneUnLoaded() => !IsSceneTransitioning;
    public static void ReloadCurrentScene() => LoadScene(ActiveScene.buildIndex);
    public static void LoadScene(string SceneName) => LoadScene(SceneManager.GetSceneByName(SceneName).buildIndex);
    public static void LoadScene(int SceneIndex) {
        IsSceneTransitioning = true;
        if (LoadSceneAsyncCoroutine != null)
            GameManager.instance.StopCoroutine(LoadSceneAsyncCoroutine);
        LoadSceneAsyncCoroutine = GameManager.instance.StartCoroutine(LoadSceneAsynchronously(SceneIndex));
    }
    private static Coroutine LoadSceneAsyncCoroutine;
    private static IEnumerator LoadSceneAsynchronously(int SceneIndex)
    {        
#if UNITY_EDITOR
        UnityEditor.Selection.activeObject = null;
#endif        
        LoadSceneAsyncHandler?.Invoke(0);
        BeginLoadSceneAsync();
        yield return GameManager.ui_Manager.Loading.BeginLoad();
        var operation = SceneManager.LoadSceneAsync(SceneIndex,LoadSceneMode.Single);
        operation.allowSceneActivation = false;
        while (!operation.isDone)
        {
            LoadSceneAsyncHandler?.Invoke(Mathf.Clamp01(operation.progress / 0.9f) * 0.8f);

            if (operation.progress >= 0.9f)
            {
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
    }
    private static void BeginLoadSceneAsync()
    {
        BeginLoadSceneAsyncHandler?.Invoke();
    }
    private static void AfterLoadSceneAsync()
    {
        GameManager.instance.Start();
        GameManager.ui_Manager.Start();
        AfterLoadSceneAsyncHandler?.Invoke();
        instance.OnAfterLoadSceneAsync();
        SoundManager.instance.Start();
    }
    public static void RegistryLoadEvents()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }
    private static void OnSceneUnloaded(Scene scene)
    {
        if (GameManager.IsQuit)
            return;
        CameraController.RemoveInstanceCache();
        PoolManager.RemoveInstanceCache();
        RemoveInstanceCache();
        IsSceneTransitioning = false;
    }

    #endregion
}