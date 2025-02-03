using System.Collections.Generic;
using System.Linq;
using UnityEngine;
//using DG.Tweening;
using System.Collections;
//using UnityEngine.Rendering.Universal;
using Yu5h1Lib.Runtime;
using Yu5h1Lib;
using UnityEngine.Events;

[RequireComponent(typeof(Camera))]
public class CameraController : SingletonBehaviour<CameraController>
{
    public event UnityAction<SpriteRenderer,Color,float> OverrideFadeMethod;
    private Camera _camera;
#pragma warning disable 0109
    public new Camera camera { 
        get {
            if (!_camera)
                _camera = GetComponent<Camera>();
            return _camera; 
        }
    } 
#pragma warning restore 0109

    public Transform Target 
    {
        get => Targets.IsEmpty() ? null : Targets.First();
        set{

            var current = Target;
            if (current == value)
                return;
            if (Targets.Count == 0)
                Targets.Add(value);
            else
                Targets[0] = value;
        } 
    }

    [SerializeField]
    private List<Transform> Targets = new List<Transform>();

    public Vector3 followOffset = new Vector3(0,0.75f,-5);
    public float bg_depth = 5;

    public Dictionary<string,SpriteRenderer> SortingLayerSprites = new Dictionary<string, SpriteRenderer>();


    
    [Range(0.05f,1)]
    public float smoothTime = 0.1f;

    public MinMax OrthoSize= new MinMax(1, 5f);
    public MinMax CamHeightRange = new MinMax(0.2f,2f);
    public float dollySpeed = 0.1f;
    //public Rect ScreenInteractionArea;

    #region cache

    private float dollyProportion = 0.75f;
    private Vector3 currentVelocity;
    public bool IsPerforming { get; private set; }
    public Timer timer;
    private Coroutine performCoroutine;
    [SerializeField]
    private SpriteRenderer _cursorRendererSource;
    [SerializeField, ReadOnly]
    private SpriteRenderer _cursorRenderer;
    public SpriteRenderer cursorRenderer
    {
        get
        {
            if (_cursorRenderer == null || !_cursorRenderer.gameObject.IsBelongToActiveScene())
            {
                if (_cursorRendererSource)
                    _cursorRenderer = Instantiate(_cursorRendererSource);
                else
                    _cursorRenderer = new GameObject("Cursor Renderer").AddComponent<SpriteRenderer>();
            }
            return _cursorRenderer;
        }
    }
    #endregion

    public ParticleSystem Cursor_Fx;

//#pragma warning disable 0109
//    public new Light2D light;
//#pragma warning restore 0109


    [SerializeField]
    private bool _cursorVisible = true;
    public bool cursorVisible
    {
        get => _cursorVisible; 
        set {
            if (_cursorVisible == value)
                return;
            _cursorVisible = value;
            if (_cursorRenderer)
                _cursorRenderer.enabled = cursorVisible;
        }
    }
    public Sprite cursorSprite { get => cursorRenderer.sprite;
        set {
            if (cursorSprite == value)
                return;
            cursorRenderer.sprite = value;
            cursorRenderer.enabled = cursorVisible && value;
        }
    }
    public Sprite squareSprite => Resources.Load<Sprite>("Texture/Square");

    private bool NeedUpdateBoard;



    #region Field
    public bool syncAngle;
    public float targetsMargin = 1;
    #endregion


    [SerializeField]
    private AnimationCurve MovementCurve = AnimationCurve.Linear(0,0,1,1);


    protected override void Init(){
        _cursorRendererSource = Resources.Load<SpriteRenderer>("UI/Cursor");
    }
    public void Start()
    {
        camera.tag = "MainCamera";
        //if (SceneController.IsLevelScene || GameObject.FindGameObjectWithTag("Player") != null) {
        //    PrepareSortingLayerSprites();
        //}
    }
    private void Update()
    {
        //if ((Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0) && ScreenInteractionArea != Rect.zero)
        //    InteractPointerCameraPosition();
        var delta = Input.mouseScrollDelta.y;
        if (!Input.GetKey(KeyCode.LeftControl) && delta != 0)
            Dolly(delta);

        var worldPosition = GetMousePositionOnCamera();
        if (Input.GetMouseButtonDown(0))
            PlayCursorEffect(worldPosition);

        if (!cursorVisible || !cursorRenderer || !cursorRenderer.sprite)
            return;
        cursorRenderer.transform.position = worldPosition;

    }
    private void FixedUpdate()
    {
        if (Target == null)
            return;
        if (IsPerforming)
            return;
        Follow();
    }
    private void LateUpdate()
    {
        if (!NeedUpdateBoard)
            return;
        FitfadeBoardWithOrthographic();
        NeedUpdateBoard = false;
    }
    public void PrepareSortingLayerSprites()
    {
        //var URP_data = camera.GetUniversalAdditionalCameraData();
        //URP_data.volumeLayerMask = 1 << LayerMask.NameToLayer("PostProcess");
        //URP_data.renderPostProcessing = true;
        camera.nearClipPlane = 0.01f;
        Dolly(0);
        camera.GetOrthographicSize(out float width, out float height);
        SortingLayerSprites = SortingLayer.layers.Select(layer => layer.name).
            ToDictionary(layerName => layerName, layerName => {
                if (!transform.TryGetComponentInChildren(layerName, out SpriteRenderer s))
                    s = GameObjectUtility.Create<SpriteRenderer>(transform);
                s.sprite = squareSprite;
                s.transform.localPosition = new Vector3(0, 0, bg_depth);
                s.sortingLayerName = s.gameObject.name = layerName;
                s.sortingOrder = 1;
                s.gameObject.SetActive(false);
                s.transform.localScale = new Vector3(width + 1, height + 1, 1);
                return s;
            });
    }
    public Vector3 GetMousePositionOnCamera(float? depth = null)
    {
        var pos = Input.mousePosition;
        if (!camera.orthographic)
            pos.z = depth ?? -camera.transform.position.z;
        return camera.ScreenToWorldPoint(pos);
    }
    public void Dolly(float delta)
    {
        dollyProportion = Mathf.Clamp(dollyProportion -= delta * dollySpeed,0,1);
        NeedUpdateBoard = true;
        //FitfadeBoardWithOrthographic();
    }
    public void PlayCursorEffect(Vector3 mouseWorldPoint)
    {
        if (!Cursor_Fx)
            Cursor_Fx = ResourcesUtility.InstantiateFromResourecs<ParticleSystem>($"UI/{nameof(Cursor_Fx)}",transform);
        Cursor_Fx.transform.position = mouseWorldPoint;
        Cursor_Fx.Play();
    }
    private void FitfadeBoardWithOrthographic()
    {
        float width = 0, height = 0;
        if (camera.orthographic)
            camera.GetOrthographicSize(out width, out height);
        else
            camera.GetPerspectiveSize(bg_depth, out width, out height);
        foreach (var renderer in SortingLayerSprites.Values)
            SetSpriteSizeByProjection(renderer, bg_depth, width, height);
    }
    public void FitSpriteWithProjection(SpriteRenderer renderer,float depth)
    {
        float width = 0, height = 0;
        if (camera.orthographic)
        {
            camera.GetOrthographicSize(out width, out height);
            SetSpriteSizeByProjection(renderer,depth, width, height);
        }
        else
        {
            camera.GetPerspectiveSize(depth, out width, out height);
            SetSpriteSizeByProjection(renderer,depth,width,height);
        }
    }
    public void SetSpriteSizeByProjection(SpriteRenderer renderer, float depth,float width,float height)
    {
        if (camera.orthographic)
            renderer.transform.localScale = new Vector3(width + 1, height + 1, 1);
        else
        {
            var sSize = renderer.sprite.bounds.size;
            Vector3 newScale = renderer.transform.localScale;
            newScale.x = width / sSize.x;
            newScale.y = height / sSize.y;
            renderer.transform.localScale = newScale;
        }
        if (renderer.transform.parent == transform)
        {
            var p = renderer.transform.localPosition;
            p.z = depth;
            renderer.transform.localPosition = p;
        }
        else
        {
            renderer.transform.rotation = transform.rotation;
            renderer.transform.position = transform.TransformPoint(0, 0, depth);
        }
    }
    [ContextMenu(nameof(FoldUp))]
    public void FoldUp()
       => FoldUp("Default", Color.black, 0.5f);
    public void FoldUp(string sortingLayerName,float duration)
       => FoldUp(sortingLayerName, Color.black, duration);

    public SpriteRenderer FoldUp(string sortingLayerName,Color color, float duration,Sprite sprite = null)
    {
        if (SortingLayerSprites.IsEmpty())
            PrepareSortingLayerSprites();
        if (!SortingLayerSprites.ContainsKey(sortingLayerName))
        {
            Debug.LogWarning($"Fade-in with layer nameed ({sortingLayerName}) does not exsits !");
            return null;
        }
        var s = SortingLayerSprites[sortingLayerName];

        sprite = sprite ?? squareSprite;

        if (s.sprite != sprite)
        {
            s.sprite = sprite;
            FitfadeBoardWithOrthographic();
        }

        color.a = 0;
        s.color = color;
        color.a = 1;
        s.gameObject.SetActive(true);
        if (OverrideFadeMethod == null)
        { 
            
        }else
            OverrideFadeMethod?.Invoke(s,color, duration);
        return s;
    }
    #region Functions of Target 
    public void ChangeTarget(Collider2D collider) => Target = collider.transform;
    public void ChangeTarget(Transform target) => Target = target;
    public void AddTarget(Transform target)
    {
        if (Targets.Contains(target))
            return;
        Targets.Add(target);
    }
    public void RemoveTarget(Transform target)
    {
        if (!Targets.Contains(target))
            return;
        Targets.Remove(target);
    }
    private float GetZLocation() => Mathf.Lerp(OrthoSize.Min, OrthoSize.Max, dollyProportion) * 2;
    /// <summary>
    /// SmoothDamp position
    /// </summary>
    private void Follow()
    {
        var center = Target.position;

        var extraDistance = 0f;
        if (Targets.Count > 1)
        {
            var bounds = new Bounds(center, Vector3.zero);
            for (int i = 1; i < Targets.Count; i++)
                center += Targets[i].position;
            center /= Targets.Count;

            for (int i = 1; i < Targets.Count; i++)
                bounds.Encapsulate(Targets[i].position + ((Targets[i].position - center).normalized * targetsMargin));

            float maxBoundsSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);

            float aspectRatio = (float)Screen.width / Screen.height;
            float fovY = camera.fieldOfView * Mathf.Deg2Rad;
            float fovX = 2 * Mathf.Atan(Mathf.Tan(fovY / 2) * aspectRatio);
            float distanceX = (bounds.size.x / 2) / Mathf.Tan(fovX / 2);
            float distanceY = (bounds.size.y / 2) / Mathf.Tan(fovY / 2);
            extraDistance = Mathf.Max(distanceX, distanceY);

            //for orthographic
            camera.orthographicSize = (bounds.size.y / 2) + targetsMargin;
        }

        camera.orthographicSize = Mathf.Lerp(OrthoSize.Min, OrthoSize.Max, dollyProportion);
        followOffset.y = Mathf.Lerp(CamHeightRange.Min, CamHeightRange.Max, dollyProportion);
        followOffset.z = -GetZLocation();

        camera.transform.position = Vector3.SmoothDamp(camera.transform.position, center + followOffset - new Vector3(0,0, extraDistance), ref currentVelocity, smoothTime);

        if (!syncAngle)
            return;
        var angles = transform.eulerAngles;
        angles.z = Target.eulerAngles.z;
        transform.eulerAngles = angles * Target.forward.z;
    }

    //public void InteractPointerCameraPosition()
    //{
    //    var c = camera.GetNormalizedCoordinates(Input.mousePosition);
    //    var pos = camera.transform.position;
    //    pos.x = ScreenInteractionArea.x + c.x * ScreenInteractionArea.width;
    //    pos.y = ScreenInteractionArea.y + c.y * ScreenInteractionArea.height;
    //    camera.transform.position = pos;
    //}
 
    
    public void Focus(Vector3 point)
    {
        camera.transform.position = point + followOffset;
    }
    public void Focus(Transform target) 
    {
        if (target)
            Focus(target.position);
    }
    public void Focus(Collider2D target)
    {
        if (target)
            Focus(target.transform);
    }
    public void PerformFocus(Collider2D collider)
    {
        if (collider)
            PerformFocus(collider.bounds.center);
    }
    public void PerformFocus(Transform target)
    {
        if (target)
            PerformFocus(target.transform.position);
    }
    public void PerformFocusCenter(Transform target)
    {
        if (target && Target)
            PerformFocus((target.transform.position + Target.transform.position) / 2);
    }
    public void PerformFocus(Vector3 pos)
        => this.StartCoroutine(ref performCoroutine, PerformFocusProcess(pos));
    IEnumerator PerformFocusProcess(Vector3 point)
    {
        timer.duration = MovementCurve.keys.Last().time;
        //timer.useUnscaledTime = true;
        timer.Start();
        IsPerforming = true;
        while (!timer.IsCompleted)
        {
            timer.Tick();
            camera.transform.position = Vector3.Lerp(camera.transform.position, point + followOffset, MovementCurve.Evaluate(timer.normalized));
            yield return null;
        }
        IsPerforming = false;
    }
    IEnumerator Perform(Vector2 point, float duration)
    {
        timer.duration = duration;
        timer.Start();
        IsPerforming = true;
        while (!timer.IsCompleted)
        {
            timer.Tick();
            yield return null;
        }
        IsPerforming = false;
    }

    #endregion

}
