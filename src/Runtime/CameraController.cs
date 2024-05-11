using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Camera))]
public class CameraController : SingletonComponent<CameraController>
{
    public Vector3 followOffset = new Vector3(0,0.75f,-5);
    private Camera _camera;
    public new Camera camera => _camera ?? (_camera = GetComponent<Camera>());
    public Transform Target;
    public Dictionary<string,SpriteRenderer> SortingLayerSprites = new Dictionary<string, SpriteRenderer>();

    
    [Range(0.05f,1)]
    public float smoothTime = 0.1f;

    public float nearestOrthographicSize = 1;
    public float nearestCameraHeight = 0.15f;

    public float farestOrthographicSize = 3.5f;
    public float farestCameraHeight = 1;

    public float zoomSpeed = 0.5f;
    public Rect ScreenInteractionArea;

    #region cache
    private Vector3 currentPosition;
    #endregion

    public void Start()
    {
        camera.tag = "MainCamera";
        if (SceneController.IsLevelScene) {
            camera.orthographic = true;
            camera.nearClipPlane = 0;
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
    }
    private void FixedUpdate()
    {

        if (Target == null)
            return;
        FollowTarget();
    }
    public void ZoomCamera(float delta)
    {
        camera.orthographicSize -= delta;
        camera.orthographicSize = Mathf.Clamp(camera.orthographicSize, nearestOrthographicSize, farestOrthographicSize);
        var normal = (camera.orthographicSize - nearestOrthographicSize).GetNormal(farestOrthographicSize - nearestOrthographicSize);
        followOffset.y = Mathf.Lerp(nearestCameraHeight, farestCameraHeight, normal);
        if (!camera.orthographic)
        {
            var nearest = nearestOrthographicSize * 2;
            var farest = farestOrthographicSize * 2;
            followOffset.z = Mathf.Clamp(followOffset.z += Mathf.Sign(delta) * zoomSpeed, -farest, -nearest);
        }
        FitfadeBoardWithOrthographic();
    }

    private void FitfadeBoardWithOrthographic()
    {
        camera.GetOrthographicSize(out float width, out float height);
        foreach (var item in SortingLayerSprites)
            item.Value.transform.localScale = new Vector3(width + 1, height + 1, 1);
    }
    public void FadeIn(string sortingLayerName,float duration)
    {
        if (!SortingLayerSprites.ContainsKey(sortingLayerName))
        {
            Debug.LogWarning($"Fadein with layer Nameed({sortingLayerName}) does not exsits !");
            return;
        }
        var s = SortingLayerSprites[sortingLayerName];
        s.color = Color.clear;
        s.gameObject.SetActive(true);
        s.DOColor(Color.black, duration);
    }
    /// <summary>
    /// SmoothDamp position
    /// </summary>
    public void FollowTarget()
    {
        camera.transform.position = Vector3.SmoothDamp(camera.transform.position, Target.position + followOffset, ref currentPosition, smoothTime);

        var angles = transform.eulerAngles;
        angles.z = Target.eulerAngles.z;
        transform.eulerAngles = angles * Target.forward.z;
    }
    public void SetTarget(Transform target,bool moveCamera = true)
    {
        Target = target;
        if (moveCamera)
        {
            var pos = Target.position;
            pos.z = camera.transform.position.z;
            camera.transform.position = Target.position + followOffset;
        }
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
