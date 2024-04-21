using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Yu5h1Lib;
using static Yu5h1Lib.GameManager.IDispatcher;

public class SceneController : MonoBehaviour
{    
    #region Scene Preset

    void Reset()
    {
    }
    #endregion
    private void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("Boundary");
        Debug.Log(gameManager);
        if (SceneController.IsLevelScene)
            Debug.Log(PoolManager.instance);
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        // Avoiding OnTriggerExit2D triggered on EditorApplication.Exit
        if (SceneController.IsLoading)
            return;
        if (other.TryGetComponent(out AttributeStatBehaviour attributeBeahaviour))
            gameManager.Despawn(attributeBeahaviour.gameObject, DespawnReason.OutOfBounds);
    }
    #region Loading stuffs...
    public static event UnityAction BeginLoadSceneHandler;
    public static event UnityAction<float> LoadSceneAsyncHandler;
    public static event UnityAction EndLoadSceneHandler;
    public static Scene ActiveScene => SceneManager.GetActiveScene();
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
        StringSearchOption.EndsWith => ActiveScene.name.Contains(name),
        _ => ActiveScene.name.Equals(name)
    };
    public static bool IsStartScene => IsSceneName("Start");
    public static bool IsLevelScene => IsSceneName("Level",StringSearchOption.StartsWith);

    public static bool IsLoading;
    public static void ReloadCurrentScene() => LoadScene(ActiveScene.buildIndex);
    public static void LoadScene(string SceneName) => LoadScene(SceneManager.GetSceneByName(SceneName).buildIndex);
    public static void LoadScene(int SceneIndex) {
        IsLoading = true;
        gameManager.StartCoroutine(LoadSceneAsynchronously(SceneIndex));
        IsLoading = false;
    } 
    public static void ExitGame()
    {
        IsLoading = true;
        BeginLoadSceneHandler?.Invoke();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
    }
    private static IEnumerator LoadSceneAsynchronously(int SceneIndex)
    {        
        BeginLoadSceneHandler?.Invoke();
        Time.timeScale = 1;
        uiManager.Loading?.gameObject.SetActive(true);
        var LoadingAsyncOperation = SceneManager.LoadSceneAsync(SceneIndex,LoadSceneMode.Single);
        while (!LoadingAsyncOperation.isDone)
        {
            LoadSceneAsyncHandler?.Invoke(Mathf.Clamp01(LoadingAsyncOperation.progress / 0.9f));
            yield return null;
        }
        uiManager.Loading?.gameObject.SetActive(false);
        EndLoadSceneHandler?.Invoke();

    }    
    #endregion
}