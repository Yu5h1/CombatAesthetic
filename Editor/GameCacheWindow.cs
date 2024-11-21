using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Yu5h1Lib.Game.Character;

namespace Yu5h1Lib
{
    public class GameCacheWindow : EditorWindow
    {
        private static GameCacheWindow m_GameCacheEditor;
        private static GameCacheWindow window
        {
            get
            {
                if (m_GameCacheEditor == null) Init();
                return m_GameCacheEditor;
            }
        }
        public class CheckPointData
        {
            public string name;
            public Vector3 position;

            public CheckPointData(string Name,Vector3 Position)
            {
                name = Name;
                position = Position;
            }
        }

        public Dictionary<string, CheckPointData[]> checkPoints;

   


        [MenuItem("Game Manager/Game Cache Editor")]
        public static void Init()
        {
            m_GameCacheEditor = (GameCacheWindow)EditorWindow.GetWindow(typeof(GameCacheWindow));
            window.titleContent = new GUIContent("GameCacheEditor");
        }

        private void OnEnable()
        {
            //if (EditorBuildSettings.scenes.Length < 1)
            //    return;
            //for (int i = 1; i < EditorBuildSettings.scenes.Length-1; i++)
            //{

            //    var path = EditorBuildSettings.scenes[i].path;
                
            //    var level = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            //    level.FindObjectsByType<CheckPoint>().Length.print();
            //    //checkPoints.Add(PathInfo.GetName(path), level.FindObjectsByType<CheckPoint>().Select(
            //    //    c => new CheckPointData(c.name, c.transform.position)).ToArray());
            //    EditorSceneManager.CloseScene(level, true);
            //}
        }
        void OnGUI()
        {
            Controller2D.gravityScale = EditorGUILayout.FloatField(nameof(Controller2D.gravityScale),Controller2D.gravityScale);

            //foreach (var cp in checkPoints.First().Value)
            //    GUILayout.Button(cp.name);


            //EditorGUILayout.ObjectField(LevelsLabel);

            //var checkedScene = EditorGUILayout.IntField("Scene", CheckPoint.scene);
            //EditorGUILayout.Popup(CheckPoint.scene,)
            //if (GUI.changed)
            //{
            //    bool IsChanged = false;
            //    if (checkedScene != CheckPoint.scene)
            //    {
            //        CheckPoint.scene = checkedScene;
            //        IsChanged = true;
            //    }
            //}
        }
    }

}