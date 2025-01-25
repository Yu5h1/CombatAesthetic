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

    [SerializeField]
    private Transform _target;
    public Transform target 
    {
        get => _target;
        set{
            if (_target == value)
                return;
            _target = value;
        } 
    }

    public Vector3 followOffset = new Vector3(0,0.75f,-5);
    public float bg_depth = 5;

    public Dictionary<string,SpriteRenderer> SortingLayerSprites = new Dictionary<string, SpriteRenderer>();

    public bool syncAngle;
    
    [Range(0.05f,1)]
    public float smoothTime = 0.1f;

    public MinMax OrthoSize= new MinMax(1, 5f);
    public MinMax CamHeightRange = new MinMax(0.2f,2f);

    private float zoomProportion = 0.75f;
    public float zoomSpeed = 0.5f;
    public Rect ScreenInteractionArea;

    #region cache
    private Vector3 currentVelocity;
    #endregion

    public ParticleSystem Cursor_Fx;

//#pragma warning disable 0109
//    public new Light2D light;
//#pragma warning restore 0109

    [ReadOnly]
    public SpriteRenderer _cursorRendererSource;
    [ReadOnly]
    public SpriteRenderer _cursorRenderer;
    public SpriteRenderer cursorRenderer { 
        get {
            if ((_cursorRenderer == null || !_cursorRenderer.gameObject.IsBelongToActiveScene()) && _cursorRendererSource )
                _cursorRenderer = Instantiate(_cursorRendererSource);
            return _cursorRenderer;
        }
    }
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

    private bool NeedUpdateZoom;

    private Timer processTimer;
    private Coroutine processCoroutine;

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

        var worldPosition = GetMousePositionOnCamera();
        if (Input.GetMouseButtonDown(0))
            PlayCursorEffect(worldPosition);

        if (!cursorVisible || !cursorRenderer || !cursorRenderer.sprite)
            return;
        cursorRenderer.transform.position = worldPosition;

    }
    private void FixedUpdate()
    {
        if (target == null)
            return;
        FollowTarget();
    }
    private void LateUpdate()
    {
        if (!NeedUpdateZoom)
            return;
        FitfadeBoardWithOrthographic();
        NeedUpdateZoom = false;
    }
    public void PrepareSortingLayerSprites()
    {
        //var URP_data = camera.GetUniversalAdditionalCameraData();
        //URP_data.volumeLayerMask = 1 << LayerMask.NameToLayer("PostProcess");
        //URP_data.renderPostProcessing = true;
        camera.nearClipPlane = 0.01f;
        ZoomCamera(0);
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
    public void ZoomCamera(float delta)
    {
        zoomProportion = Mathf.Clamp(zoomProportion -= delta * zoomSpeed,0,1);
        camera.orthographicSize = Mathf.Lerp(OrthoSize.Min, OrthoSize.Max, zoomProportion); 
        followOffset.y = Mathf.Lerp(CamHeightRange.Min, CamHeightRange.Max, zoomProportion);
        if (!camera.orthographic)
            followOffset.z = -Mathf.Lerp(OrthoSize.Min, OrthoSize.Max,zoomProportion) * 2;

        NeedUpdateZoom = true;
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
        //var tween = s.DOColor(color, duration);
        //tween.SetUpdate(true);
        return s;
    }

    IEnumerator perform(float duration)
    {
        float lastTime = Time.time + duration;

        while (Time.time < lastTime)
        { 
            yield return null;
        }

        yield return null;
    }



    /// <summary>
    /// SmoothDamp position
    /// </summary>
    public void FollowTarget()
    {
        camera.transform.position = Vector3.SmoothDamp(camera.transform.position, target.position + followOffset, ref currentVelocity, smoothTime);

        if (!syncAngle)
            return;
        var angles = transform.eulerAngles;
        angles.z = target.eulerAngles.z;
        transform.eulerAngles = angles * target.forward.z;
    }
    public void Focus()
    {
        if (!target)
            return;
        var pos = target.position;
        pos.z = camera.transform.position.z;
        camera.transform.position = target.position + followOffset;
    }
    public void InteractPointerCameraPosition()
    {
        var c = camera.GetNormalizedCoordinates(Input.mousePosition);
        var pos = camera.transform.position;
        pos.x = ScreenInteractionArea.x + c.x * ScreenInteractionArea.width;
        pos.y = ScreenInteractionArea.y + c.y * ScreenInteractionArea.height;
        camera.transform.position = pos;
    }
}
