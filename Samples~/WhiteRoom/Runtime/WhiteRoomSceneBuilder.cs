using System;
using System.Collections.Generic;
using UnityEngine;

namespace Yu5h1Lib.CombatAesthetic.Samples.WhiteRoom
{
    public enum ArenaScenario
    {
        OnePlayerVsOneAI = 0,    // 1P vs 1 AI（預設單挑測試）
        OnePlayerVsTwoAI = 1,    // 1P vs 2 AI（多對手圍攻測試）
        TwoPlayerVsTwoAI = 2,    // 2P vs 2 AI（雙人合作 vs AI 傀儡）
        TwoPlayerPvP = 3         // 2P PvP（同機/雙人互打測試）
    }

    public class WhiteRoomSceneBuilder : MonoBehaviour
    {
        [SerializeField] private bool _autoSetupOnAwake = true;
        [SerializeField] private ArenaScenario _initialScenario = ArenaScenario.OnePlayerVsOneAI;

        private ArenaScenario _currentScenario = ArenaScenario.OnePlayerVsOneAI;
        public ArenaScenario CurrentScenario => _currentScenario;

        private readonly List<GameObject> _spawnedEntities = new List<GameObject>();

        private void Awake()
        {
            if (_autoSetupOnAwake)
            {
                SetupEnvironment();
                SetupHUD();
                ApplyScenario(_initialScenario);
            }
        }

        public void SetupEnvironment()
        {
            // Floor (50m x 50m White Room)
            if (GameObject.Find("WhiteRoom_Floor") == null)
            {
                GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
                floor.name = "WhiteRoom_Floor";
                floor.transform.position = Vector3.zero;
                floor.transform.localScale = new Vector3(5f, 1f, 5f);

                Material floorMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse"));
                floorMat.color = new Color(0.88f, 0.90f, 0.92f);
                floor.GetComponent<Renderer>().sharedMaterial = floorMat;
            }

            // Directional Light
            if (FindAnyObjectByType<Light>() == null)
            {
                GameObject lightGo = new GameObject("Directional Light");
                Light light = lightGo.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.2f;
                light.color = new Color(1f, 0.98f, 0.95f);
                lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            }
        }

        public void SetupHUD()
        {
            WhiteRoomHUD hud = FindAnyObjectByType<WhiteRoomHUD>();
            if (hud == null)
            {
                GameObject hudGo = new GameObject("WhiteRoomHUD");
                hudGo.AddComponent<WhiteRoomHUD>();
            }
        }

        public void ApplyScenario(ArenaScenario scenario)
        {
            _currentScenario = scenario;

            // 1. 清除先前動態生成的角色
            foreach (var go in _spawnedEntities)
            {
                if (go != null) SafeDestroy(go);
            }
            _spawnedEntities.Clear();

            // 2. 清理場景中任何舊有的 CombatEntity（確保純淨初始狀態）
            var oldEntities = FindObjectsByType<CombatEntity>();
            foreach (var ent in oldEntities)
            {
                if (ent != null && ent.gameObject != null)
                {
                    SafeDestroy(ent.gameObject);
                }
            }

            WhiteRoomCharacter p1 = null;

            // 3. 依據情境配置生成角色
            switch (scenario)
            {
                case ArenaScenario.OnePlayerVsOneAI:
                    p1 = SpawnPlayer("Player1", 0, new Vector3(0, 0, -3.5f), Quaternion.identity, false, new Color(0.1f, 0.7f, 1.0f));
                    SpawnAI("AI_Dummy1", 1, new Vector3(0, 0, 3.5f), Quaternion.Euler(0, 180f, 0), new Color(1f, 0.25f, 0.25f), new Color(0.35f, 0.38f, 0.42f));
                    break;

                case ArenaScenario.OnePlayerVsTwoAI:
                    p1 = SpawnPlayer("Player1", 0, new Vector3(0, 0, -3.5f), Quaternion.identity, false, new Color(0.1f, 0.7f, 1.0f));
                    SpawnAI("AI_Dummy1", 1, new Vector3(-2.2f, 0, 3.5f), Quaternion.Euler(0, 180f, 0), new Color(1f, 0.25f, 0.25f), new Color(0.35f, 0.38f, 0.42f));
                    SpawnAI("AI_Dummy2", 1, new Vector3(2.2f, 0, 3.5f), Quaternion.Euler(0, 180f, 0), new Color(1f, 0.55f, 0.1f), new Color(0.28f, 0.30f, 0.35f));
                    break;

                case ArenaScenario.TwoPlayerVsTwoAI:
                    p1 = SpawnPlayer("Player1", 0, new Vector3(-1.8f, 0, -3.5f), Quaternion.identity, false, new Color(0.1f, 0.7f, 1.0f));
                    SpawnPlayer("Player2", 0, new Vector3(1.8f, 0, -3.5f), Quaternion.identity, true, new Color(0.2f, 0.9f, 0.4f));
                    SpawnAI("AI_Dummy1", 1, new Vector3(-1.8f, 0, 3.5f), Quaternion.Euler(0, 180f, 0), new Color(1f, 0.25f, 0.25f), new Color(0.35f, 0.38f, 0.42f));
                    SpawnAI("AI_Dummy2", 1, new Vector3(1.8f, 0, 3.5f), Quaternion.Euler(0, 180f, 0), new Color(1f, 0.55f, 0.1f), new Color(0.28f, 0.30f, 0.35f));
                    break;

                case ArenaScenario.TwoPlayerPvP:
                    p1 = SpawnPlayer("Player1", 0, new Vector3(0, 0, -3.5f), Quaternion.identity, false, new Color(0.1f, 0.7f, 1.0f));
                    SpawnPlayer("Player2", 1, new Vector3(0, 0, 3.5f), Quaternion.Euler(0, 180f, 0), true, new Color(1f, 0.3f, 0.3f));
                    break;
            }

            // 4. 配置第三人稱鏡頭追蹤 P1
            SetupCamera(p1);

            // 5. 通知 HUD 刷新綁定
            WhiteRoomHUD hud = FindAnyObjectByType<WhiteRoomHUD>();
            if (hud != null)
            {
                hud.RefreshBindings();
            }
        }

        private WhiteRoomCharacter SpawnPlayer(string id, int team, Vector3 position, Quaternion rotation, bool isPlayer2, Color visorColor)
        {
            GameObject playerGo = new GameObject(id);
            playerGo.transform.position = position;
            playerGo.transform.rotation = rotation;

            CombatEntity entity = playerGo.AddComponent<CombatEntity>();
            entity.InitializeSettings(id, team, 100f);

            WhiteRoomCharacter player = playerGo.AddComponent<WhiteRoomCharacter>();
            player.SetControlScheme(isPlayer2);

            GameObject visuals = WhiteRoomVisuals.CreateRobotModel(
                playerGo.transform,
                out Transform fist,
                out Renderer visor,
                out Renderer torso
            );

            visor.material.color = visorColor;

            entity.VisualRoot = visuals.transform;
            entity.FistTransform = fist;

            WhiteRoomVisualFeedback feedback = playerGo.AddComponent<WhiteRoomVisualFeedback>();
            feedback.BindVisuals(visuals.transform, fist, visuals.transform.Find("Head"), visor, torso);

            _spawnedEntities.Add(playerGo);
            return player;
        }

        private WhiteRoomDummy SpawnAI(string id, int team, Vector3 position, Quaternion rotation, Color visorColor, Color torsoColor)
        {
            GameObject dummyGo = new GameObject(id);
            dummyGo.transform.position = position;
            dummyGo.transform.rotation = rotation;

            CombatEntity entity = dummyGo.AddComponent<CombatEntity>();
            entity.InitializeSettings(id, team, 100f);

            WhiteRoomDummy dummy = dummyGo.AddComponent<WhiteRoomDummy>();

            GameObject visuals = WhiteRoomVisuals.CreateRobotModel(
                dummyGo.transform,
                out Transform fist,
                out Renderer visor,
                out Renderer torso
            );

            torso.material.color = torsoColor;
            visor.material.color = visorColor;

            entity.VisualRoot = visuals.transform;
            entity.FistTransform = fist;

            WhiteRoomVisualFeedback feedback = dummyGo.AddComponent<WhiteRoomVisualFeedback>();
            feedback.BindVisuals(visuals.transform, fist, visuals.transform.Find("Head"), visor, torso);

            _spawnedEntities.Add(dummyGo);
            return dummy;
        }

        private void SetupCamera(WhiteRoomCharacter targetPlayer)
        {
            if (targetPlayer == null) return;

            WhiteRoomCamera cam = FindAnyObjectByType<WhiteRoomCamera>();
            if (cam == null)
            {
                Camera mainCam = Camera.main;
                GameObject camGo = mainCam != null ? mainCam.gameObject : new GameObject("Main Camera", typeof(Camera));
                if (mainCam == null)
                {
                    camGo.tag = "MainCamera";
                    camGo.AddComponent<AudioListener>();
                }
                cam = camGo.AddComponent<WhiteRoomCamera>();
            }
            cam.Target = targetPlayer.transform;
        }

        private void SafeDestroy(GameObject go)
        {
            if (go == null) return;
            if (Application.isPlaying) Destroy(go);
            else DestroyImmediate(go);
        }
    }
}