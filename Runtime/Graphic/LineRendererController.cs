using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Yu5h1Lib;
using Yu5h1Lib.Runtime;

public class LineRendererController : ComponentController<LineRenderer>
{
    public enum Style
    {
        Shrink = -1,
        Extend = 1,
    }
    public LineRenderer lineRenderer => component;

    [SerializeField,Range(0,10)]
    private int segmentsPerSegment = 5;

    [SerializeField]
    private List<Transform> targets;

    private Vector3[] positionsCache;

    public LayerMask layer;
    public TagOption tagOption;

    [SerializeField, ReadOnly]
    private bool _IsConnecting;
    public bool IsConnecting
    {
        get => _IsConnecting; 
        set{
            if (_IsConnecting == value)
                return;
            _IsConnecting = value;
            if (value)
                Connect();
            else 
                Disconnect(3);
        }
    }


    private bool _IsHit;
    private bool IsHit{
        get => _IsHit;
        set
        {
            if (_IsHit == value)
                return;
            _IsHit = value;
            OnHitStateChanged();
        }
    }
    [SerializeField] 
    private MinMax depthRange;
    [SerializeField]
    private bool breakOnHit = true;
    [SerializeField]
    private float ConnectDelay = 0.5f;
    [SerializeField]
    private float ConnectDuration = 0.5f;

    #region Event Fields
    [SerializeField]
    private UnityEvent _HitStateChanged;
    public event UnityAction HitStateChanged
    {
        add => _HitStateChanged.AddListener(value);
        remove => _HitStateChanged.RemoveListener(value);
    }
    [SerializeField]
    private UnityEvent _connected;
    public event UnityAction connected
    {
        add => _connected.AddListener(value);
        remove => _connected.RemoveListener(value);
    }
    [SerializeField]
    private UnityEvent _disconnected;
    public event UnityAction disconnected
    {
        add => _disconnected.AddListener(value);
        remove => _disconnected.RemoveListener(value);
    }
    #endregion
    public RaycastHit2D hitInfo { get; private set; }


    private float performingTimer;
    [ReadOnly]
    public bool IsPerforming;//{ get; private set; }

    private Coroutine performCoroutine;

    protected override void Start()
    {
        base.Start();
        Connect();
    }
    private void FixedUpdate()
    {
        Refresh();
    }
    [ContextMenu(nameof(Refresh))]
    public void Refresh()
    {
        if (!IsConnecting || targets.IsEmpty() || targets.Count < 2) 
            return;

        bool shouldLoop = lineRenderer.loop && targets.Count > 2;

        int segmentCount = targets.Count - (shouldLoop ? 0 : 1);
        int requiredSize = targets.Count;

        if (segmentsPerSegment > 0)
            requiredSize += segmentCount * segmentsPerSegment;

        if (positionsCache == null || positionsCache.Length != requiredSize)
            positionsCache = new Vector3[requiredSize];

        int index = 0;
        hitInfo = default(RaycastHit2D);
        for (int i = 0; i < targets.Count - 1; i++)
        {
            hitInfo = ProcessSegment(targets[i].position, targets[i + 1].position, ref index);
            if (hitInfo)
                break;
        }
        //var lastPos = hitInfo ? (Vector3)hitInfo.point : targets[targets.Count - 1].position;

        var lastPos = targets[targets.Count - 1].position;

        if (shouldLoop)
        {
            if (!hitInfo)
                ProcessSegment(lastPos, targets[0].position, ref index);

        }else
            positionsCache[positionsCache.Length - 1] = lastPos;

        IsHit = hitInfo;


        lineRenderer.positionCount = requiredSize;
        lineRenderer.SetPositions(positionsCache);
    }
    RaycastHit2D ProcessSegment(Vector3 start, Vector3 end, ref int index)
    {
        positionsCache[index++] = start;
        var result = default(RaycastHit2D);

        if (!IsPerforming)
        {
            var hit = depthRange.Length > 0 ?
                Physics2D.Linecast(start, end, layer,depthRange.Min,depthRange.Max):
                Physics2D.Linecast(start, end, layer);
#if UNITY_EDITOR
            if (hit)
                Debug.DrawLine(start, hit.point);
#endif
            if (hit && tagOption.Compare(hit.transform.gameObject))
                result = hit;
        }
   
            
        if (segmentsPerSegment > 0)
        {
            var interval = 1.0f / (segmentsPerSegment + 1);
            var t = interval;
            for (int j = 0; j < segmentsPerSegment; j++)
            {
                positionsCache[index++] = Vector3.Lerp(start, end, t);
                t += interval;
            }
        }

        return result;
        //if (hit && tagOption.Compare(hit.transform.gameObject))
        //    return hit;
        //return default(RaycastHit2D);
    }
    #region Event

    protected void OnHitStateChanged()
    {
        if (breakOnHit)
        {
            IsConnecting = false;
        }
        _HitStateChanged?.Invoke();
    }
    private void OnDestroy()
    {
    }
    #endregion
    void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        if (targets != null)
        {
            foreach (var target in targets)
            {
                if (target != null)
                    Gizmos.DrawSphere(target.position, 0.1f);
            }
        }
    }
    private void InvokeDisconnected() => _disconnected?.Invoke();
    private void InvokeConnected() => _connected?.Invoke();
    #region Coroutine
    [ContextMenu(nameof(Connect))]
    public void Connect()
    {        
        _IsConnecting = true;
        _IsHit = false;
        this.StartCoroutine(ref performCoroutine,
            FadeProcess(ConnectDelay, ConnectDuration, 1, defaultColor: new Color(1, 1, 1, 0), performEnd: InvokeConnected));
    }
    [ContextMenu(nameof(Disconnect))]
    public void Disconnect() => IsConnecting = false;

    public void Disconnect(int index, Style style = Style.Extend)
    {
        _IsConnecting = false;
        var duration = 0.3f;
        this.StartCoroutine(ref performCoroutine, FadeProcess(0, duration, 0, index, style,performEnd: InvokeDisconnected));
    }

    private IEnumerator FadeProcess(float delay, float duration, float alpha, int startIndex = 0,
            Style style = Style.Extend, Color? defaultColor = null ,
            Action performBegin = null, Action performEnd = null)
    {
        lineRenderer.PrepareGradient(defaultColor);
        if (delay > 0)
            yield return new WaitForSeconds(delay);
        performingTimer = Time.time;
        IsPerforming = true;
        performBegin?.Invoke();

        var colorGradient = lineRenderer.colorGradient;
        var durationInterval = duration / 8;

        var max = Mathf.Max(startIndex, 8 - startIndex);
        var backward_i = style == Style.Shrink ? 7 : startIndex;
        var forward_i = style == Style.Shrink ? 0 : startIndex;

        for (int i = 0; i < max; i++)
        {
            SetColorKey(forward_i, alpha, durationInterval);
            SetColorKey(backward_i, alpha, durationInterval);
            forward_i++;
            backward_i--;
            yield return new WaitForSeconds(durationInterval);
        }

        IsPerforming = false;
        performEnd?.Invoke();

    }
    private void SetColorKey(int index, float alpha, float duration)
    {
        if (index < 0 || index > 7)
            return;
        var g = lineRenderer.colorGradient;
        var alphaKeys = g.alphaKeys;
        var colorKeys = g.colorKeys;
        alphaKeys[index].alpha = alpha;
        g.SetKeys(colorKeys, alphaKeys);
        lineRenderer.colorGradient = g;
    }
    private IEnumerator DelaySetColorKey(int index, float alpha, float duration)
    {
        SetColorKey(index, alpha, duration);
        yield return new WaitForSeconds(duration);
    }
    public void Log(string msg) => msg.print();
    #endregion
}
