using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Yu5h1Lib;

public class SceneController : SingletonComponent<SceneController>
{
    public string[] StartLines;

    #region Scene Preset
    void Reset()
    {
    }
    #endregion
    private void Start()
    {
        //forceCreateGameManager
        Debug.Log(GameManager.instance);
        gameObject.layer = LayerMask.NameToLayer("Boundary");
        if (SceneController.IsLevelScene)
        {
            if (!StartLines.IsEmpty())
            {
                GameManager.IsGamePause = true;
                GameManager.ui_Manager.Dialog_UI.lines = StartLines;
                GameManager.ui_Manager.Dialog_UI.gameObject.SetActive(true);
            }
            Debug.Log($"{PoolManager.instance} was Created.\n{PoolManager.canvas}");
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        // Avoiding OnTriggerExit2D triggered on EditorApplication.Exit
        if (GameManager.IsQuit || IsSceneTransitioning)
            return;
        if (other.TryGetComponent(out AttributeBehaviour attributeBeahaviour))
            attributeBeahaviour.Affect(AttributeType.Health, AffectType.NEGATIVE, 1000);
    }
    #region Static 
    public static event UnityAction BeginLoadSceneHandler;
    public static event UnityAction<float> LoadSceneAsyncHandler;
    public static event UnityAction EndLoadSceneHandler;
    public static Scene ActiveScene => SceneManager.GetActiveScene();
    public static bool BelongToCurrentScene(GameObject gameObject) => gameObject.scene == ActiveScene;


    public static bool IsSceneName(string name, StringSearchOption comparison = StringSearchOption.Equals) => comparison switch
    {
        StringSearchOption.StartsWith => ActiveScene.name.StartsWith(name),
        StringSearchOption.Contains => ActiveScene.name.Contains(name),
        StringSearchOption.EndsWith => ActiveScene.name.Contains(name),
        _ => ActiveScene.name.Equals(name)
    };
    public static bool IsStartScene => IsSceneName("Start");
    public static bool IsLevelScene => IsSceneName("Level",StringSearchOption.StartsWith);

    public static bool IsSceneTransitioning;
    private static bool IsSceneUnLoaded() => !IsSceneTransitioning;
    public static void ReloadCurrentScene() => LoadScene(ActiveScene.buildIndex);
    public static void LoadScene(string SceneName) => LoadScene(SceneManager.GetSceneByName(SceneName).buildIndex);
    public static void LoadScene(int SceneIndex) {
        IsSceneTransitioning = true;
        GameManager.instance.StartCoroutine(LoadSceneAsynchronously(SceneIndex));
    } 
    private static IEnumerator LoadSceneAsynchronously(int SceneIndex)
    {
        BeginLoadSceneAsync();
        Time.timeScale = 1;
        GameManager.ui_Manager.Loading?.gameObject.SetActive(true);
        var LoadingAsyncOperation = SceneManager.LoadSceneAsync(SceneIndex,LoadSceneMode.Single);
        while (!LoadingAsyncOperation.isDone)
        {
            LoadSceneAsyncHandler?.Invoke(Mathf.Clamp01(LoadingAsyncOperation.progress / 0.9f));
            yield return null;
        }
        yield return new WaitUntil(IsSceneUnLoaded);
        GameManager.ui_Manager.Loading?.gameObject.SetActive(false);
        EndLoadSceneAsync();
    }
    private static void BeginLoadSceneAsync()
    {
        BeginLoadSceneHandler?.Invoke();
    }
    private static void EndLoadSceneAsync()
    {
        EndLoadSceneHandler?.Invoke();
        GameManager.ui_Manager.Start();
        GameManager.instance.Start();
    }
    public static void RegistryLoadEvents()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }
    private static void OnSceneUnloaded(Scene scene)
    {
        CameraController.RemoveInstanceCache();
        PoolManager.RemoveInstanceCache();
        DG.Tweening.DOTween.KillAll();
        IsSceneTransitioning = false;
    }
    #endregion
}