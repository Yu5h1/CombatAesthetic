using UnityEngine;

namespace Yu5h1Lib.CombatAesthetic.Samples.WhiteRoom
{
    public class WhiteRoomSceneBuilder : MonoBehaviour
    {
        [SerializeField] private bool _autoSetupOnAwake = true;

        private void Awake()
        {
            if (_autoSetupOnAwake)
            {
                SetupEnvironment();
                SetupPlayer();
                SetupDummy();
                SetupHUD();
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
            if (FindFirstObjectByType<Light>() == null)
            {
                GameObject lightGo = new GameObject("Directional Light");
                Light light = lightGo.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.2f;
                light.color = new Color(1f, 0.98f, 0.95f);
                lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            }
        }

        public void SetupPlayer()
        {
            WhiteRoomCharacter player = FindFirstObjectByType<WhiteRoomCharacter>();
            if (player == null)
            {
                GameObject playerGo = new GameObject("PlayerCharacter");
                playerGo.transform.position = new Vector3(0, 0, -3.5f);

                CombatEntity entity = playerGo.AddComponent<CombatEntity>();
                entity.InitializeSettings("Player1", 0, 100f);

                player = playerGo.AddComponent<WhiteRoomCharacter>();

                // Procedural Robot Visuals
                GameObject visuals = WhiteRoomVisuals.CreateRobotModel(
                    playerGo.transform,
                    out Transform fist,
                    out Renderer visor,
                    out Renderer torso
                );

                entity.VisualRoot = visuals.transform;
                entity.FistTransform = fist;

                WhiteRoomVisualFeedback feedback = playerGo.AddComponent<WhiteRoomVisualFeedback>();
                feedback.BindVisuals(visuals.transform, fist, visuals.transform.Find("Head"), visor, torso);
            }

            // Orbital Third-Person Camera
            WhiteRoomCamera cam = FindFirstObjectByType<WhiteRoomCamera>();
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
                cam.Target = player.transform;
            }
            else
            {
                cam.Target = player.transform;
            }
        }

        public void SetupDummy()
        {
            WhiteRoomDummy dummy = FindFirstObjectByType<WhiteRoomDummy>();
            if (dummy == null)
            {
                GameObject dummyGo = new GameObject("TrainingDummy");
                dummyGo.transform.position = new Vector3(0, 0, 3.5f);
                dummyGo.transform.rotation = Quaternion.Euler(0, 180f, 0);

                CombatEntity entity = dummyGo.AddComponent<CombatEntity>();
                entity.InitializeSettings("AI_Dummy", 1, 100f);

                dummy = dummyGo.AddComponent<WhiteRoomDummy>();

                // Procedural Robot Visuals for AI
                GameObject visuals = WhiteRoomVisuals.CreateRobotModel(
                    dummyGo.transform,
                    out Transform fist,
                    out Renderer visor,
                    out Renderer torso
                );

                // 給予對手深灰金屬軀幹以示區別
                torso.material.color = new Color(0.35f, 0.38f, 0.42f);
                visor.material.color = new Color(1f, 0.3f, 0.2f); // 紅色視鏡識別敵對

                entity.VisualRoot = visuals.transform;
                entity.FistTransform = fist;

                WhiteRoomVisualFeedback feedback = dummyGo.AddComponent<WhiteRoomVisualFeedback>();
                feedback.BindVisuals(visuals.transform, fist, visuals.transform.Find("Head"), visor, torso);
            }
        }

        public void SetupHUD()
        {
            WhiteRoomHUD hud = FindFirstObjectByType<WhiteRoomHUD>();
            if (hud == null)
            {
                GameObject hudGo = new GameObject("WhiteRoomHUD");
                hudGo.AddComponent<WhiteRoomHUD>();
            }
        }
    }
}
