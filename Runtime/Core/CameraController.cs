using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;
using Yu5h1Lib.Runtime;
using Yu5h1Lib;
using UnityEngine.Events;

[RequireComponent(typeof(Camera))]
public class CameraController : SingletonBehaviour<CameraController>
{
    public event UnityAction<SpriteRenderer,Color,float> OverrideFadeMethod;
    [SerializeField,ReadOnly]
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
    public static bool IsPerforming { get; private set; }

    private float dollyProportion = 0.75f;
    private Vector3 currentVelocity;
    
    public Timer timer { get; private set; } = new Timer();
    public Timer.Wait waiter;

    private Coroutine coroutineCache;
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
    private AnimationCurve AnimatedShotCurve = AnimationCurve.Linear(0,0,1,1);

    public bool follow = true;

    public bool allowStopPerformance = true;

    public UnityEvent<Camera> _start;


    protected override void OnInstantiated() { }
    protected override void OnInitializing()
    {
        _cursorRendererSource = Resources.Load<SpriteRenderer>("UI/Cursor");
        IsPerforming = false;
    }
    public void Start()
    {
        camera.tag = "MainCamera";
        //if (SceneController.IsLevelScene || GameObject.FindGameObjectWithTag("Player") != null) {
        //    PrepareSortingLayerSprites();
        //}
        waiter = timer.Waiting();
        _start?.Invoke(camera);
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
        if (IsPerforming)
            return;
        if (Target == null || !follow)
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

    public void FitSpriteWithProjection(SpriteRenderer renderer, float depth, float scaleMultiplier)
    {
        float width = 0, height = 0;
        if (camera.orthographic)
        {
            camera.GetOrthographicSize(out width, out height);
            SetSpriteSizeByProjection(renderer, depth, width, height, scaleMultiplier);
        }
        else
        {
            camera.GetPerspectiveSize(depth, out width, out height);
            SetSpriteSizeByProjection(renderer, depth, width, height, scaleMultiplier);
        }
    }
    public void SetSpriteSizeByProjection(SpriteRenderer renderer, float depth, float width, float height,
        float scaleMultiplier )
    {
        if (renderer == null || renderer.sprite == null)
            return;

        var spriteSize = renderer.sprite.bounds.size;
        if (spriteSize.x == 0 || spriteSize.y == 0)
            return; // 防止除以零

        Vector3 newScale = renderer.transform.localScale;

        if (camera.orthographic)
        {
            // 正交模式：直接以目標寬度或高度為準縮放
            float scaleFactor = Mathf.Min(width / spriteSize.x, height / spriteSize.y);
            newScale = Vector3.one + (Vector3.one * scaleFactor * scaleMultiplier);
        }
        else
        {
            // 透視模式：同理
            float scaleFactor = Mathf.Min(width / spriteSize.x, height / spriteSize.y);
            newScale = Vector3.one + (Vector3.one * scaleFactor * scaleMultiplier);
        }

        renderer.transform.localScale = newScale;

        // 設定位置
        if (renderer.transform.parent == transform)
        {
            var pos = renderer.transform.localPosition;
            pos.z = depth;
            renderer.transform.localPosition = pos;
        }
        else
        {
            renderer.transform.rotation = transform.rotation;
            var pos = renderer.transform.position;
            pos.z = depth;
            renderer.transform.position = pos;
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
        camera.transform.position = Vector3.SmoothDamp(camera.transform.position, GetFollowPoint(), ref currentVelocity, smoothTime);
        if (!syncAngle)
            return;
        var angles = transform.eulerAngles;
        angles.z = Target.eulerAngles.z;
        transform.eulerAngles = angles * Target.forward.z;
    }

    public Vector3 GetFollowPoint()
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

            center.z -= extraDistance;
        }

        camera.orthographicSize = Mathf.Lerp(OrthoSize.Min, OrthoSize.Max, dollyProportion);
        followOffset.y = Mathf.Lerp(CamHeightRange.Min, CamHeightRange.Max, dollyProportion);
        followOffset.z = -GetZLocation();

        return center + followOffset;
    }
    public Vector3 GetCenter(Vector3[] points)
    {        
        var result = Vector3.zero;
        if (points.IsEmpty())
            return result;
        if (points.Length == 1)
            return points[0];

        var center = points[0];

        var bounds = new Bounds(center, Vector3.zero);
        
        for (int i = 1; i < points.Length; i++)
            center += points[i];
        center /= points.Length;

        for (int i = 1; i < points.Length; i++)
            bounds.Encapsulate(points[i] + ((points[i] - center).normalized * targetsMargin));

        float maxBoundsSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);

        float aspectRatio = (float)Screen.width / Screen.height;
        float fovY = camera.fieldOfView * Mathf.Deg2Rad;
        float fovX = 2 * Mathf.Atan(Mathf.Tan(fovY / 2) * aspectRatio);
        float distanceX = (bounds.size.x / 2) / Mathf.Tan(fovX / 2);
        float distanceY = (bounds.size.y / 2) / Mathf.Tan(fovY / 2);
        center.z = -Mathf.Max(distanceX, distanceY);
        return center;
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
    public void Focus(AnimatedInfo info, System.Func<Vector3> To,UnityAction completed)
        => this.StartCoroutine(ref coroutineCache, FocusProcess(info,To, completed));
    public void Focus(Collider2D collider)
    {
        if (collider)
            Focus(collider.bounds.center);
    }
    public void StopPerformance() => StopPerformance(default,null);
    public void StopPerformance(AnimatedInfo info,UnityAction completed)
    {
        if (!allowStopPerformance)
            return;
        if (info.duration == 0)
        {
            if (coroutineCache != null)
                StopCoroutine(coroutineCache);
            IsPerforming = false;
            transform.position = GetFollowPoint();
            follow = true;
            completed?.Invoke();
            return;
        }
        this.StartCoroutine(ref coroutineCache, StopFocusProcess(info, completed));
    } 
    public void StopPerformance(UnityAction completed)
        => StopPerformance(AnimatedInfo.Default, completed);

    #endregion

    #region Animated

    private Vector3[] positions;
    private Vector3 GetPosition() => transform.position;
    private Vector3 GetCenter()
    {
        if (positions.IsEmpty())
            positions = new Vector3[Targets.Count];
        else if (Targets.Count != positions.Length)
            System.Array.Resize(ref positions, Targets.Count);
        for (int i = 0; i < Targets.Count ; i++)
            positions[i] = Targets[i].position;
        return GetCenter(positions);
    }
    private Vector3 animatedStartPoint;
    private Vector3 GetAnimatedStartPoint() => animatedStartPoint;
    private float GetNormalizedTime(float current) => current / timer.duration;
    public IEnumerator FocusProcess(System.Func<Vector3> From, System.Func<Vector3> To, UnityAction completed,
            AnimatedInfo info = default(AnimatedInfo),AnimationCurve curve = null)
    {
        curve ??= AnimatedShotCurve;
        IsPerforming = true;


        if (info.duration == 0 && curve.keys.IsEmpty())
            yield break;
        if (info.delay > 0)
            yield return new WaitForSeconds(info.delay);

        bool IsValidCurve = curve == null && curve.keys.Length > 1;

        timer.duration = info.duration > 0 ? info.duration : curve.keys[^1].time;
        timer.useUnscaledTime = true;
        timer.Start();

        animatedStartPoint = From();
        var GetFrom = info.keepTracking ? From : GetAnimatedStartPoint;
        System.Func<float,float> GetT = IsValidCurve ? curve.Evaluate : GetNormalizedTime;
        while (!timer.IsCompleted)
        {
            timer.Tick();
            camera.transform.position = Vector3.Lerp(GetFrom(),
            To(), GetT(timer.time));
            yield return null;
        }
        IsPerforming = false;
        completed?.Invoke();
    }
    [System.Serializable]
    public struct AnimatedInfo 
    {
        public float delay;
        public float duration;
        public bool keepTracking;
        public static AnimatedInfo Default = new AnimatedInfo() { delay = 0, duration = 1, keepTracking = false };
    }

    IEnumerator FocusProcess(AnimatedInfo animatedInfo,System.Func<Vector3> To, UnityAction completed)
    {
        follow = false;
        yield return FocusProcess(GetPosition, To, completed, animatedInfo);
    }

    IEnumerator StopFocusProcess(AnimatedInfo animatedInfo,UnityAction completed)
    {
        allowStopPerformance = false;
        yield return FocusProcess(GetPosition, GetFollowPoint, completed, animatedInfo);
        currentVelocity = Vector3.zero;
        follow = true;
        allowStopPerformance = true;
    }
    #endregion

#if UNITY_EDITOR
    //[SerializeField]
    //private float guiScaleFactor = 3.14f;
    //private void OnGUI()
    //{
    //    var originalmatrix = GUI.matrix;
    //    float matrixScale = Mathf.Min(Screen.width / 1920f, Screen.height / 1080f) * guiScaleFactor;
    //    GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(matrixScale, matrixScale, 1));
    //    GUILayout.Label($"Timer:{timer}");

    //    GUI.matrix = originalmatrix;
    //}

    private void OnDrawGizmos()
    {
        if (camera == null || Mathf.Abs(camera.transform.forward.z) != 1f) return;

        using (new Scopes.GizmosScope(Color.yellow))
        {
            var zDistance = -camera.transform.position.z;

            float fov = camera.fieldOfView;
            float aspect = camera.aspect;

            float height = 2f * Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad) * zDistance;
            float width = height * aspect;

            Vector3 center = camera.transform.position + camera.transform.forward * zDistance;
            Vector3 up = camera.transform.up * (height / 2f);
            Vector3 right = camera.transform.right * (width / 2f);

            Vector3 topLeft = center + up - right;
            Vector3 topRight = center + up + right;
            Vector3 bottomLeft = center - up - right;
            Vector3 bottomRight = center - up + right;

            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topRight, bottomRight);
            Gizmos.DrawLine(bottomRight, bottomLeft);
            Gizmos.DrawLine(bottomLeft, topLeft);
        }

    }
#endif
}
