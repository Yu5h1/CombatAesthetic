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
            // Floor
            if (GameObject.Find("WhiteRoom_Floor") == null)
            {
                GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
                floor.name = "WhiteRoom_Floor";
                floor.transform.position = Vector3.zero;
                floor.transform.localScale = new Vector3(5f, 1f, 5f); // 50m x 50m arena

                Material floorMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse"));
                floorMat.color = new Color(0.88f, 0.9f, 0.92f);
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
                playerGo.transform.position = new Vector3(0, 0, -4f);
                player = playerGo.AddComponent<WhiteRoomCharacter>();

                GameObject visuals = WhiteRoomVisuals.CreateRobotModel(playerGo.transform, out Transform fist);

                // Setup private fields via reflection or properties if needed, or serialize
                var fieldVisualRoot = typeof(WhiteRoomCharacter).GetField("_visualRoot", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                fieldVisualRoot?.SetValue(player, visuals.transform);

                var fieldFist = typeof(WhiteRoomCharacter).GetField("_rightFist", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                fieldFist?.SetValue(player, fist);
            }

            // Camera
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
                dummyGo.transform.position = new Vector3(0, 0, 3f);
                dummyGo.transform.rotation = Quaternion.Euler(0, 180f, 0);

                CharacterController cc = dummyGo.AddComponent<CharacterController>();
                cc.height = 2f;
                cc.center = new Vector3(0, 1f, 0);

                dummy = dummyGo.AddComponent<WhiteRoomDummy>();

                // Visual body
                GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                body.name = "DummyVisual";
                body.transform.SetParent(dummyGo.transform, false);
                body.transform.localPosition = new Vector3(0, 1f, 0);
                body.transform.localScale = new Vector3(0.7f, 1f, 0.7f);
                Object.DestroyImmediate(body.GetComponent<Collider>());

                Material dummyMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse"));
                dummyMat.color = new Color(0.85f, 0.85f, 0.85f);
                Renderer rend = body.GetComponent<Renderer>();
                rend.sharedMaterial = dummyMat;

                var rendField = typeof(WhiteRoomDummy).GetField("_renderer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                rendField?.SetValue(dummy, rend);
            }
        }

        public void SetupHUD()
        {
            if (FindFirstObjectByType<WhiteRoomHUD>() == null)
            {
                GameObject hudGo = new GameObject("WhiteRoomHUD");
                hudGo.AddComponent<WhiteRoomHUD>();
            }
        }
    }
}
