using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Camera))]
public class CameraController : SingletonComponent<CameraController>
{
    private Camera _camera;
    public new Camera camera => _camera ?? (_camera = GetComponent<Camera>());

    public Transform target;

    public Dictionary<string,SpriteRenderer> SortingLayerSprites = new Dictionary<string, SpriteRenderer>();

    [Range(0.05f,1)]
    public float smoothTime = 0.1f;

    #region property
    private Vector3 CurrentVelocity;
    #endregion

    public void Start()
    {
        camera.orthographic = true;
        camera.nearClipPlane = 0;

        camera.GetOrthographicSize(out float width, out float height);
        SortingLayerSprites = SortingLayer.layers.Select(layer => layer.name).
            ToDictionary(layerName => layerName , layerName => {
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
    private void FixedUpdate()
    {
        if (target == null)
            return;
        FollowTarget();
    }
    public void ZoomCamera(float delta)
    {
        camera.orthographicSize -= delta;
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
        camera.transform.position = Vector3.SmoothDamp(camera.transform.position, target.position - camera.transform.forward , ref CurrentVelocity, smoothTime);
    }
}
