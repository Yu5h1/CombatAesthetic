using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Yu5h1Lib;
using Yu5h1Lib.EditorExtension;

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
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    if (enableClearLevelCacheOnExitGame)
                    {
                        ClearLevelCache();                        
                        SceneController.ClearLoadAsyncEvent();
                    }
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
        public const string ToggleDebugMode_Label = "Game Manager/DebugMode";
        [MenuItem(ToggleDebugMode_Label)]
        private static void ToggleDebugMode() => GameManager.DebugMode = !GameManager.DebugMode;
        [MenuItem(ToggleDebugMode_Label, true)]
        private static bool ToggleDebugModeItemChecked()
        {
            Menu.SetChecked(ToggleDebugMode_Label, GameManager.DebugMode);
            return true;
        }

        [MenuItem("Game Manager/Clear Level Cache")]
        public static void ClearLevelCache()
        {
            TeleportGate2D.GateStates.Clear();
            CheckPoint.Clear();
        }
        public static bool enableClearLevelCacheOnExitGame = true;
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