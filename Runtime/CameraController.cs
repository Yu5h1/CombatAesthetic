using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using System.Collections;
using UnityEngine.Rendering.Universal;
using Yu5h1Lib.Runtime;
using Yu5h1Lib;

[RequireComponent(typeof(Camera))]
public class CameraController : SingletonBehaviour<CameraController>
{
    public Vector3 followOffset = new Vector3(0,0.75f,-5);
    private Camera _camera;
#pragma warning disable 0109
    public new Camera camera => _camera ?? (_camera = GetComponent<Camera>());
#pragma warning restore 0109
    public Transform Target;
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

#pragma warning disable 0109
    public new Light2D light;
#pragma warning restore 0109

    [ReadOnly]
    public SpriteRenderer _cursorRendererSource;
    [ReadOnly]
    public SpriteRenderer _cursorRenderer;
    public SpriteRenderer cursorRenderer { 
        get {
            if ((_cursorRenderer == null || !_cursorRenderer.gameObject.IsBelongToActiveScene()) && _cursorRendererSource )
            {
                _cursorRenderer = Instantiate(_cursorRendererSource);
                _cursorRenderer.gameObject.SetActive(cursorVisible);
            }
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
                _cursorRenderer.gameObject.SetActive(cursorVisible);
        }
    }
    public Sprite cursorSprite { get => cursorRenderer.sprite; set => cursorRenderer.sprite = value; }

    protected override void Init(){
        _cursorRendererSource = Resources.Load<SpriteRenderer>("UI/Cursor");

    }

    public void Start()
    {
        camera.tag = "MainCamera";
        if (SceneController.IsLevelScene) {
            var URP_data = camera.GetUniversalAdditionalCameraData();
            URP_data.volumeLayerMask = 1 << LayerMask.NameToLayer("PostProcess");
            URP_data.renderPostProcessing = true;
            camera.nearClipPlane = 0.01f;
            ZoomCamera(0);
            camera.GetOrthographicSize(out float width, out float height);
            SortingLayerSprites = SortingLayer.layers.Select(layer => layer.name).
                ToDictionary(layerName => layerName, layerName => {
                    var s = GameObjectEx.Create<SpriteRenderer>(transform);
                    s.sprite = Resources.Load<Sprite>("Texture/Square");
                    s.transform.localPosition = Vector3.forward;
                    s.sortingLayerName = s.gameObject.name = layerName;
                    s.sortingOrder = 1;
                    s.gameObject.SetActive(false);
                    s.transform.localScale = new Vector3(width + 1, height + 1, 1);
                    return s;
                });
        }
    }
    private void Update()
    {
        //if ((Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0) && ScreenInteractionArea != Rect.zero)
        //    InteractPointerCameraPosition();



        if (!cursorVisible || !cursorRenderer || !cursorRenderer.sprite)
            return;
        var worldPosition = GetMousePositionOnCamera();
        cursorRenderer.transform.position = worldPosition;
    }
    private void FixedUpdate()
    {

        if (Target == null)
            return;
        FollowTarget();
    }
    public Vector3 GetMousePositionOnCamera()
    {
        var pos = Input.mousePosition;
        if (!camera.orthographic)
            pos.z = -camera.transform.position.z;
        return camera.ScreenToWorldPoint(pos);
    }
    public void ZoomCamera(float delta)
    {
        zoomProportion = Mathf.Clamp(zoomProportion -= delta * zoomSpeed,0,1);
        camera.orthographicSize = Mathf.Lerp(OrthoSize.Min, OrthoSize.Max, zoomProportion); 
        followOffset.y = Mathf.Lerp(CamHeightRange.Min, CamHeightRange.Max, zoomProportion);
        if (!camera.orthographic)
            followOffset.z = -Mathf.Lerp(OrthoSize.Min, OrthoSize.Max,zoomProportion) * 2;

        FitfadeBoardWithOrthographic();
    }
    public void PlayCursorEffect()
    {
        if (!Cursor_Fx)
            Cursor_Fx = GameObjectEx.InstantiateFromResourecs<ParticleSystem>($"UI/{nameof(Cursor_Fx)}",transform);

        var pos = Input.mousePosition;
        if (!camera.orthographic)
            pos.z = 1;
        var mouseWorldPosition = camera.ScreenToWorldPoint(pos);
        Cursor_Fx.transform.position = mouseWorldPosition;
        Cursor_Fx.Play();
    }
    private void FitfadeBoardWithOrthographic()
    {
        camera.GetOrthographicSize(out float width, out float height);
        foreach (var item in SortingLayerSprites)
            item.Value.transform.localScale = new Vector3(width + 1, height + 1, 1);
    }
    public void FadeIn(string sortingLayerName,float duration)
       => FadeIn(sortingLayerName, Color.black, duration);

    public void FadeIn(string sortingLayerName,Color color, float duration)
    {
        if (!SortingLayerSprites.ContainsKey(sortingLayerName))
        {
            Debug.LogWarning($"Fade-in with layer nameed ({sortingLayerName}) does not exsits !");
            return;
        }
        var s = SortingLayerSprites[sortingLayerName];
        color.a = 0;
        s.color = color;
        color.a = 1;
        s.gameObject.SetActive(true);
        var tween = s.DOColor(color, duration);
        tween.SetUpdate(true);
    }

    /// <summary>
    /// SmoothDamp position
    /// </summary>
    public void FollowTarget()
    {
        camera.transform.position = Vector3.SmoothDamp(camera.transform.position, Target.position + followOffset, ref currentVelocity, smoothTime);

        if (!syncAngle)
            return;
        var angles = transform.eulerAngles;
        angles.z = Target.eulerAngles.z;
        transform.eulerAngles = angles * Target.forward.z;
    }
    public void SetTarget(Transform target,bool focus = true)
    {
        Target = target;
        if (focus)
            Focus();
    }
    public void Focus()
    {
        var pos = Target.position;
        pos.z = camera.transform.position.z;
        camera.transform.position = Target.position + followOffset;
    }
    public void InteractPointerCameraPosition()
    {
        var c = camera.GetNormalizedCoordinates(Input.mousePosition);
        var pos = camera.transform.position;
        pos.x = ScreenInteractionArea.x + c.x * ScreenInteractionArea.width;
        pos.y = ScreenInteractionArea.y + c.y * ScreenInteractionArea.height;
        camera.transform.position = pos;
    }

    public void FollowTargetUnscaleTime()
    {


    }
    IEnumerator Method()
    {
        yield return null;
    }
}
