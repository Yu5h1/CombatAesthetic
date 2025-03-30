using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Yu5h1Lib;
using Yu5h1Lib.EditorExtension;
using Yu5h1Lib.Runtime;
using Yu5h1Lib.Game;
using UnityEngine.SceneManagement;

namespace Yu5h1Lib
{
    [CustomEditor(typeof(GameManager))]
    public class GameManagerEditor : Editor<GameManager>
    {

        [InitializeOnLoadMethod]
        private static void EditorInitialize()
        {
            EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
        }

        private static void EditorApplication_playModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredEditMode:
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    if (Utility.SceneUtility.GetSceneOfDontDestroyOnLoad(out Scene sceneOfDontDestroyOnLoad))
                        SceneVisibilityManager.instance.DisablePicking(sceneOfDontDestroyOnLoad);
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    SceneController.UnloadSingleton();
                    SceneController.ClearLoadAsyncEvent();
                    if (enableClearLevelCacheOnExitGame)
                        ClearLevelCache();

                    
                    break;
                default:
                    break;
            }
        }


        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        }
        #region MenuItems

        [MenuItem("Game Manager/Clear Level Cache")]
        public static void ClearLevelCache()
        {
            Teleporter.GateStates.Clear();
            CheckPoint.Clear();
        }
        public static bool enableClearLevelCacheOnExitGame 
        {
            get => EditorPrefs.GetBool("enableClearLevelCacheOnExitGame", false);
            set => EditorPrefs.SetBool("enableClearLevelCacheOnExitGame", value);
        }
        public const string CLCOnEG_Label = "Game Manager/Clear Cache On Exit Game";
        [MenuItem(CLCOnEG_Label)]
        private static void ClearLevelCacheOnExitGameMenuItem()
        {
            enableClearLevelCacheOnExitGame = !enableClearLevelCacheOnExitGame;
        }
        [MenuItem(CLCOnEG_Label, true)]
        private static bool ToggleActionValidateMenuItemChecked()
        {
            Menu.SetChecked(CLCOnEG_Label, enableClearLevelCacheOnExitGame);
            return true;
        }
        #endregion
    }
}