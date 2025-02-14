using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Yu5h1Lib;
using Yu5h1Lib.EditorExtension;
using UnityEditorInternal;

[CustomEditor(typeof(Teleporter))]
public class TeleportEditor : Editor<Teleporter> {
    public static float DottedLineSize = 3;

    public static Teleporter[] Teleporters;
    public static Teleporter current;

    private static bool _ShowAllHandles;
    public static bool ShowAllHandles
    {
        get => _ShowAllHandles;
        set
        {
            if (_ShowAllHandles == value)
                return;
            _ShowAllHandles = value;
            if (value)
            {
                EditorApplication.hierarchyChanged += FindTeleporters;
                Teleporters = GameObject.FindObjectsOfType<Teleporter>();
                SceneView.duringSceneGui += OnSceneGUI;
            }
            else
            {
                EditorApplication.hierarchyChanged -= FindTeleporters;
                SceneView.duringSceneGui -= OnSceneGUI;
            }
        }
    }
    [InitializeOnLoadMethod]
    static void InitializeOnLoad()
    {
        ShowAllHandles = true;
    }

    private static void FindTeleporters() => Teleporters = GameObject.FindObjectsOfType<Teleporter>();

    private static void OnSceneGUI(SceneView view)
    {
        if (Teleporters.IsEmpty())
            return;
        foreach (var teleporter in Teleporters)
            if (current != teleporter)
                foreach (var collider2D in teleporter.GetComponents<Collider2D>())
                    collider2D.DrawHandles(Color.cyan.ChangeAlpha(0.5f),HandleStyle.Button);
    }
    [MenuItem("CONTEXT/Teleporter/Show all handles")]
    private static void ShowAllHandlesToggle(MenuCommand command)
    {
        ShowAllHandles = !ShowAllHandles;
        Menu.SetChecked("CONTEXT/Teleporter/Show all handles", ShowAllHandles);
    }


    Collider2D collider;
    private void OnEnable()
    {
        collider = targetObject.GetComponent<Collider2D>();
        current = targetObject;
    }
    public override void OnInspectorGUI()
   {
		DrawDefaultInspector();
   }
    private void OnSceneGUI()
    {
        //if (!ShowAllHandles)
        //    collider.DrawHandles();
        if (!InternalEditorUtility.GetIsInspectorExpanded(target) || targetObject.loadSceneOnly)
            return;

        Handles.DrawDottedLine(targetObject.transform.position, targetObject.destination, DottedLineSize);
        var zTest = Handles.zTest;
        Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
        Vector3 newDestination = Handles.Slider2D(
            targetObject.destination,
            Vector3.forward,
            Vector3.right,
            Vector3.up,
            HandleUtility.GetHandleSize(targetObject.destination) * 0.1f,
            Handles.DotHandleCap,
            1f
        );
        Handles.zTest = zTest;
        if (targetObject.destination != (Vector2)newDestination )
        {
            Undo.RegisterCompleteObjectUndo(targetObject, "Teleporter destination Changed");
            targetObject.destination = newDestination;
        }
            
    }
    private void OnDisable()
    {
      current = null;
    }
};