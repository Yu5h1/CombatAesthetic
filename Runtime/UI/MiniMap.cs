using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Yu5h1Lib;

[RequireComponent(typeof(RectTransform))]
public class MiniMap : MonoBehaviour
{
    public readonly string Tag = $"{nameof(MiniMap)}Camera";

    public LinkedList<GameObject> objs;
    [Serializable]
    public class TagInfo
    {
        [DropDownTag]
        public string tag;
        [NoAlpha]
        public Color color;
        public float size = 20;
    }

    #region Property
    public Dictionary<Transform, Image> elements { get; private set; } = new Dictionary<Transform, Image>();
#pragma warning disable 0109
    [SerializeField, ReadOnly]
    private Camera _camera;
    public new Camera camera { get => _camera; private set => _camera = value; }
#pragma warning restore 0109
    [SerializeField, ReadOnly]
    private RectTransform _view;
    public RectTransform view { get => _view; private set => _view = value; }
    #endregion
    public TagInfo[] Tags;

    Pool imagePool;
    private void Start()
    {
        if ("The Minimap must be attached to a GameObject with a RectTransform component.".printWarningIf(!TryGetComponent(out _view)))
            return;
        if (this.TryFindGameObjectWithTag(Tag, out GameObject found))
            camera = found.GetComponent<Camera>();

        if ($"{Tag} cannot be found.".printWarningIf(!camera))
            return;

        imagePool = PoolManager.instance.Add<Image>(5, Init);
        imagePool.resizeable = true;

        foreach (var info in Tags)
            foreach (var obj in GameObject.FindGameObjectsWithTag(info.tag))
                elements.Add(obj.transform, null);
    }
    private void FixedUpdate()
    {
        if (!camera || !view || elements.IsEmpty())
            return;

        foreach (var item in elements.Reverse())
        {
            var obj = item.Key.transform;
            Vector3 screenPoint = camera.WorldToScreenPoint(obj.position);

            Vector2 minimapPos = ScreenToViewLocalPosition(screenPoint);

            //RectTransformUtility.ScreenPointToLocalPointInRectangle(
            //    view,
            //    screenPoint,
            //    null,
            //    out minimapPos
            //);

            if (screenPoint.z < 0 || !view.rect.Contains(minimapPos))
            {
                if (item.Value)
                    PoolManager.instance.Despawn(item.Value);
                elements[item.Key] = null;
                continue;
            }
            else
            {
                if (!item.Value)
                {
                    elements[item.Key] = PoolManager.instance.Spawn<Image>(parent: view.transform);

                    var color = Color.white;
                    var size = 20.0f;
                    if (Tags.TryGet(d=>d.tag == item.Key.tag,out TagInfo info)){
                        color = info.color;
                        size = info.size;
                    }
                    elements[item.Key].color = color;
                    elements[item.Key].rectTransform.sizeDelta = new Vector2(size, size);

                }
            }

            elements[item.Key].rectTransform.localPosition = minimapPos;

            // Debugging the minimap position

        }
    }
    private void Init(Image img)
    {

    }
    private Vector2 ScreenToViewLocalPosition(Vector3 screenPoint)
    {
        Rect rect = view.rect;
        float x = ((screenPoint.x / camera.pixelWidth) - view.pivot.x) * rect.width;
        float y = ((screenPoint.y / camera.pixelHeight) - view.pivot.y) * rect.height;
        return new Vector2(x, y);
    }
    private Vector2 ClampToRectBounds(Rect rect, Vector2 pos)
    {
        float clampedX = Mathf.Clamp(pos.x, rect.xMin, rect.xMax);
        float clampedY = Mathf.Clamp(pos.y, rect.yMin, rect.yMax);
        return new Vector2(clampedX, clampedY);
    }
}
