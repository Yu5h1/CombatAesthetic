using System.Collections.Generic;
using UnityEngine;
using Yu5h1Lib.CombatAesthetic.Samples.WhiteRoom;

namespace Yu5h1Lib.CombatAesthetic.Samples.WhiteRoom.Handshake
{
    /// <summary>
    /// 握手場景建造器（HandshakeSceneBuilder）：
    /// 自動建置 50m x 50m 白色測試場、第三人稱軌道鏡頭、握手判定元件 (Arbitrator)、
    /// 以及起始面對面、相距 2.4m（走得到的距離內）的機械人形玩家與 AI 對手。
    /// </summary>
    public class HandshakeSceneBuilder : MonoBehaviour
    {
        [SerializeField] private bool _autoSetupOnAwake = true;

        private readonly List<GameObject> _spawnedEntities = new List<GameObject>();

        private void Awake()
        {
            if (_autoSetupOnAwake)
            {
                SetupEnvironment();
                SetupArbitrator();
                SetupHUD();
                SpawnParticipants();
            }
        }

        public void SetupEnvironment()
        {
            // 1. 白色場地地板 (50m x 50m)
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

            // 2. 平行光源
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

        public void SetupArbitrator()
        {
            if (HandshakeArbitrator.Instance == null)
            {
                GameObject arbGo = new GameObject("HandshakeArbitrator");
                arbGo.AddComponent<HandshakeArbitrator>();
            }
        }

        public void SetupHUD()
        {
            if (FindAnyObjectByType<HandshakeHUD>() == null)
            {
                GameObject hudGo = new GameObject("HandshakeHUD");
                hudGo.AddComponent<HandshakeHUD>();
            }
        }

        public void SpawnParticipants()
        {
            // 清理舊實體
            foreach (var go in _spawnedEntities)
            {
                if (go != null) SafeDestroy(go);
            }
            _spawnedEntities.Clear();

            var oldParticipants = FindObjectsByType<HandshakeParticipant>();
            foreach (var p in oldParticipants)
            {
                if (p != null && p.gameObject != null)
                {
                    SafeDestroy(p.gameObject);
                }
            }

            // 1. 生成玩家 Player1 (Z = -1.2, 朝向 +Z)
            HandshakeCharacter player = SpawnPlayer(
                "Player1",
                0,
                new Vector3(0, 0, -1.2f),
                Quaternion.identity,
                new Color(0.1f, 0.7f, 1.0f) // 亮青色視鏡
            );

            // 2. 生成 AI 對手 (Z = +1.2, 朝向 -Z，初始相距 2.4m 面對面)
            HandshakeAiController ai = SpawnAI(
                "AI_Dummy",
                1,
                new Vector3(0, 0, 1.2f),
                Quaternion.Euler(0, 180f, 0),
                new Color(1f, 0.25f, 0.25f), // 紅色視鏡
                new Color(0.35f, 0.38f, 0.42f)
            );

            // 3. 配置鏡頭追蹤玩家
            SetupCamera(player);

            // 4. 註冊至 Arbitrator
            var arb = HandshakeArbitrator.Instance;
            if (arb != null)
            {
                arb.RegisterParticipant(player.Participant);
                arb.RegisterParticipant(ai.GetComponent<HandshakeParticipant>());
            }
        }

        private HandshakeCharacter SpawnPlayer(string id, int team, Vector3 position, Quaternion rotation, Color visorColor)
        {
            GameObject playerGo = new GameObject(id);
            playerGo.transform.position = position;
            playerGo.transform.rotation = rotation;

            HandshakeParticipant participant = playerGo.AddComponent<HandshakeParticipant>();
            participant.Initialize(id, team);

            HandshakeCharacter playerChar = playerGo.AddComponent<HandshakeCharacter>();

            GameObject visuals = WhiteRoomVisuals.CreateRobotModel(
                playerGo.transform,
                out Transform fist,
                out Renderer visor,
                out Renderer torso
            );

            visor.material.color = visorColor;
            participant.SetRightHand(fist);

            HandshakeVisualFeedback feedback = playerGo.AddComponent<HandshakeVisualFeedback>();
            feedback.BindVisuals(visuals.transform, fist, visuals.transform.Find("Head"), visor, torso);

            _spawnedEntities.Add(playerGo);
            return playerChar;
        }

        private HandshakeAiController SpawnAI(string id, int team, Vector3 position, Quaternion rotation, Color visorColor, Color torsoColor)
        {
            GameObject aiGo = new GameObject(id);
            aiGo.transform.position = position;
            aiGo.transform.rotation = rotation;

            HandshakeParticipant participant = aiGo.AddComponent<HandshakeParticipant>();
            participant.Initialize(id, team);

            HandshakeAiController aiCtrl = aiGo.AddComponent<HandshakeAiController>();

            GameObject visuals = WhiteRoomVisuals.CreateRobotModel(
                aiGo.transform,
                out Transform fist,
                out Renderer visor,
                out Renderer torso
            );

            torso.material.color = torsoColor;
            visor.material.color = visorColor;
            participant.SetRightHand(fist);

            HandshakeVisualFeedback feedback = aiGo.AddComponent<HandshakeVisualFeedback>();
            feedback.BindVisuals(visuals.transform, fist, visuals.transform.Find("Head"), visor, torso);

            _spawnedEntities.Add(aiGo);
            return aiCtrl;
        }

        private void SetupCamera(HandshakeCharacter targetPlayer)
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
