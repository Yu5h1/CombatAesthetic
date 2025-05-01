using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.Linq;
using Yu5h1Lib.EditorPrefsExtension;

namespace Yu5h1Lib
{
    using DG.Tweening;
    using EditorExtension;
    using System.Collections.Generic;

    public class PSDImporterWindow : EditorWindow
    {
        public struct InitialData
        {
            public Vector3 scale;
            public Vector3 screenPosition;
            public float depth;
            public float screenHeight;
            public InitialData(Vector3 Scale,Vector3 ScreenPosition, float Depth, float ScreenHeight)
            {
                scale = Scale;
                screenPosition = ScreenPosition;
                depth = Depth;
                screenHeight = ScreenHeight;
            }
        }
        private static PSDImporterWindow _instance;
        private static PSDImporterWindow instance
        {
            get
            {
                if (_instance == null) Init();
                return _instance;
            }
        }
        public GameObject Result;

        public const string UnpackKey = nameof(UnpackWhileInstantiatePrefab);
        public bool UnpackWhileInstantiatePrefab
        {
            get => this.GetBool(UnpackKey, true);
            set => this.SetBool(UnpackKey, value);
        }
        public static int baseOrder = -5;
        public static float depthMultiplier = 1;
        public static AnimationCurve depthCurve = new AnimationCurve(new Keyframe[]
       {
            new Keyframe(0f, 0f),
            new Keyframe(1f, 1f)
       });

        public static string[] SkipParentFolders = new string[] { "Test" };


 
        float depthFactor;
        private SpriteRenderer[] _renderers;
        private SpriteRenderer[] renderers
        {
            get
            {
                if (Result && _renderers.IsEmpty())
                {
                    _renderers = Result.GetComponentsInChildren<SpriteRenderer>();
     
                    initinalDatas = new Dictionary<Transform, InitialData>();
                    foreach (var r in _renderers)
                    {
                        var t = r.transform;
                        var initialDepth = GetDepthAlongCameraForward(t);
                        Vector3 screenPos = depthCamera.WorldToScreenPoint(t.position);                        
                        initinalDatas.Add(r.transform, new InitialData(t.localScale, new Vector3(screenPos.x, screenPos.y, initialDepth), GetDepthAlongCameraForward(t), GetPerspectiveHeightAtDepth(initialDepth)));
                    }
                }
                return _renderers;
            }
        }
        Dictionary<Transform, InitialData> initinalDatas;
   
        private bool renderersFoldout;

        private float scaleMultiplier = 1;

        private Vector2 scrollPos;

        private Camera _depthCamera;
        private Camera depthCamera
        { 
            get
            { 
                if (_depthCamera == null)
                    _depthCamera = FindAnyObjectByType<Camera>();
                return _depthCamera;
            }
        }

        [MenuItem("Tools/PSDImporterWindow")]
        public static void Init()
        {
            _instance = (PSDImporterWindow)EditorWindow.GetWindow(typeof(PSDImporterWindow));
            instance.titleContent = new GUIContent("PSDImporterWindow");
            if (Selection.activeObject is GameObject source)
                instance.Result = GameObject.Find(source.name);
        }
        void OnGUI()
        {
            _depthCamera = EditorField.Draw(nameof(depthCamera), _depthCamera);

            GUILayout.Space(10);
            EditorGUI.BeginChangeCheck();
            Result = EditorField.Draw(nameof(Result), Result);
            if (EditorGUI.EndChangeCheck())
                _renderers = null;


            if (depthCamera == null)
                EditorGUILayout.HelpBox("CameraController does not exist", MessageType.Warning);
            else{
                scaleMultiplier = EditorGUILayout.FloatField(nameof(scaleMultiplier), scaleMultiplier);

                EditorGUI.BeginChangeCheck();
                depthCurve = EditorGUILayout.CurveField(nameof(depthCurve), depthCurve);
                if (EditorGUI.EndChangeCheck() && Result && !renderers.IsEmpty())
                {
                    int minOrder = renderers.Min(r => r.sortingOrder);
                    int maxOrder = renderers.Max(r => r.sortingOrder);
                    Undo.RegisterCompleteObjectUndo(Result, $"SetDepthCurve");
                    foreach (var r in renderers)
                    {
                        var pos = r.transform.position;
                        float normalized = (r.sortingOrder - minOrder) / (float)(maxOrder - minOrder);
                        pos.z = depthCurve.Evaluate(1 - normalized);
                        r.transform.position = pos;
                        UpdateSpriteTransform(r.transform, initinalDatas[r.transform]);
                    }
                }

                renderersFoldout = EditorGUILayout.Foldout(renderersFoldout, "Renderers Depth");
                if (renderersFoldout && !renderers.IsEmpty())
                {
                    scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                    foreach (var r in renderers)
                    {
                        var pos = r.transform.position;
                        pos.z = EditorGUILayout.FloatField(r.transform.name, pos.z);
                        if (pos != r.transform.position)
                        {
                            r.transform.position = pos;
                            if (!$"{r.name} does not contain initinalData.".printWarningIf(initinalDatas == null || !initinalDatas.ContainsKey(r.transform)))
                                UpdateSpriteTransform(r.transform, initinalDatas[r.transform]);
                        }
                    }
                    EditorGUILayout.EndScrollView();
                }
            }


            GUILayout.Space(10);

            UnpackWhileInstantiatePrefab = EditorGUILayout.Toggle(nameof(UnpackWhileInstantiatePrefab), UnpackWhileInstantiatePrefab);

            GUILayout.Space(10);

            using (new EditorGUI.DisabledScope(Selection.activeObject is not GameObject))
            {
                if (GUILayout.Button("Instantiate PSD Prefab and Replace Sprites"))
                {

                    var newResult = InstantiateAndReplaceSprites();
                    if (newResult != null && Result != null)
                    {
                        Transform parent = Result.transform.parent;
                        Vector3 pos = Result.transform.position;
                        Quaternion rot = Result.transform.rotation;
                        GameObject.DestroyImmediate(Result);

                        if (UnpackWhileInstantiatePrefab)
                            PrefabUtility.UnpackPrefabInstance(newResult, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                        newResult.transform.position = pos;
                        newResult.transform.rotation = rot;
                        newResult.transform.SetParent(parent,true);
                    }
                    Result = newResult;
                }

            }
            using (new EditorGUI.DisabledScope(Result == null))
            {
                using (new GUILayout.HorizontalScope())
                {
                    baseOrder = EditorGUILayout.IntField(nameof(baseOrder), baseOrder);
                    if (GUILayout.Button("ReOrder"))
                    {
                        ReOrder(baseOrder);
                    }
                }
                GUILayout.Space(10);

                if (GUILayout.Button(nameof(SetSizeByCameraProjection)))
                    SetSizeByCameraProjection();
            }
        }

        [MenuItem("Assets/Instantiate PSD Prefab and Replace Sprites", true)]
        static bool ValidateSelection()
        {
            return Selection.activeObject is GameObject;
        }


        [MenuItem("Assets/Instantiate PSD Prefab and Replace Sprites")]
        static GameObject InstantiateAndReplaceSprites()
        {
            GameObject selectedPrefab = Selection.activeObject as GameObject;
            if (selectedPrefab == null)
            {
                Debug.LogError("請選擇一個 PSD Prefab");
                return null;
            }

            string prefabPath = AssetDatabase.GetAssetPath(selectedPrefab);
            string prefabDirectory = PathInfo.Skip(Path.GetDirectoryName(prefabPath), SkipParentFolders);


            GameObject instance = PrefabUtility.InstantiatePrefab(selectedPrefab) as GameObject;

            Undo.RegisterCreatedObjectUndo(instance, "Instantiate PSD Prefab");

            SpriteRenderer[] renderers = instance.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var renderer in renderers)
            {
                string layerName = renderer.gameObject.transform.parent.GetHierarchyPath();
                layerName = PathInfo.Split(layerName).Select(d => d.TrimAfter(" _", true)).Join('/');
                string spritePath = Path.Combine(prefabDirectory, layerName, renderer.sprite.name + ".png");
                spritePath = spritePath.Replace("\\", "/");

                Sprite newSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                if (newSprite != null)
                    renderer.sprite = newSprite;
                else
                {
                    Debug.LogWarning($"找不到圖層 {layerName} 對應的 PNG：{spritePath}", renderer.gameObject);
                }
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            return instance;
        }


        public void ReOrder(int top)
        {
            if (!Result)
                return;

            var renderers = Result.GetComponentsInChildren<SpriteRenderer>();

            int minOrder = renderers.Min(r => r.sortingOrder);
            int maxOrder = renderers.Max(r => r.sortingOrder);

            int distance = maxOrder - minOrder;

            foreach (var r in renderers)
            {
                int relativePosition = r.sortingOrder - maxOrder;

                r.sortingOrder = top + relativePosition;
            }
        }
        //var pos = r.transform.position;
        //pos.z = -(r.sortingOrder * depthMultiplier);
        //r.transform.position = pos;
        [ContextMenu("Apply Depth By Sorting Order")]
        public void ApplyDepth()
        {
            if (!Result)
                return;
            SpriteRenderer[] renderers = Result.GetComponentsInChildren<SpriteRenderer>(true);

            if (renderers.Length == 0)
            {
                Debug.LogWarning("沒有找到任何 SpriteRenderer！");
                return;
            }

            int minOrder = renderers.Min(r => r.sortingOrder);
            int maxOrder = renderers.Max(r => r.sortingOrder);

            if (minOrder == maxOrder)
            {
                Debug.LogWarning("所有 SpriteRenderer sortingOrder 相同，不需要調整！");
                return;
            }

            foreach (var renderer in renderers)
            {
                float t = (renderer.sortingOrder - minOrder) / (float)(maxOrder - minOrder);
                float depth = depthCurve.Evaluate(t);

                Vector3 pos = renderer.transform.localPosition;
                pos.z = depth;
                renderer.transform.localPosition = pos;
            }

            Debug.Log("已完成依排序改 Z Depth！");
        }

        private void SetSizeByCameraProjection(params SpriteRenderer[] only)
        {
            if (!Result)
                return;
            var controller = GameObject.FindAnyObjectByType<CameraController>();
            if ("Requires CameraController implementation method".printWarningIf(!controller))
                return;
            Undo.RegisterCompleteObjectUndo(Result, $"SetSizeByCameraProjection");
            var renderers = only.IsEmpty() ? Result.GetComponentsInChildren<SpriteRenderer>(true) :
                Result.GetComponentsInChildren<SpriteRenderer>(true).Where(r => only.Contains(r));
            foreach (var r in renderers)
            {
                controller.FitSpriteWithProjection(r, r.transform.position.z, scaleMultiplier);
            }

        }
        void UpdateSpriteTransform(Transform transform, InitialData data)
        {
            if (depthCamera == null)
                return;
            Undo.RegisterCompleteObjectUndo(Result, $"SetSizeByCameraProjection");
            float currentDepth = GetDepthAlongCameraForward(transform);
            float currentScreenHeight = GetPerspectiveHeightAtDepth(currentDepth);

            float scaleMultiplier = currentScreenHeight / data.screenHeight;
            transform.localScale = data.scale * scaleMultiplier;

            Vector3 newWorldPos = depthCamera.ScreenToWorldPoint(new Vector3(data.screenPosition.x, data.screenPosition.y, currentDepth));
            transform.position = newWorldPos;
        }
        private float GetPerspectiveHeightAtDepth(float depth)
        {
            if (depthCamera.orthographic)
            {
                Debug.LogWarning("Camera is orthographic! This method is intended for perspective cameras.");
                return 1f;
            }

            float fovInRadians = depthCamera.fieldOfView * Mathf.Deg2Rad;
            float height = 2f * depth * Mathf.Tan(fovInRadians * 0.5f);
            return height;
        }
        private float GetDepthAlongCameraForward(Transform t)
        {
            Vector3 camToObj = t.position - depthCamera.transform.position;
            return Vector3.Dot(camToObj, depthCamera.transform.forward);
        }

    }
}