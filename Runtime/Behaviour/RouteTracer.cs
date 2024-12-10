using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yu5h1Lib;
using Yu5h1Lib.Game.Character;

public class RouteTracer : MonoBehaviour
{

    [SerializeField]
    private Route2D _route;
    public Route2D route => _route;

    private int _current;
    public int current { get => _current; private set => _current = value; }
    public int next => route.GetNext(current);




    
    [SerializeField]
    private Timer _timer;
    public Timer timer => _timer;
    private float totalTime;

    private Vector2 _offset;
    public Vector2 offset { get => _offset; private set => _offset = value; }

    private Vector3 currentVelocity;
    private Vector3 previousPosition;
    public Vector2 velocity { get; private set; }

    public float smoothTime = 0.1f;

    [Header("----Caches-----")]
    [SerializeField, ReadOnly]
    private float totalLength;
    [SerializeField, ReadOnly]
    private float[] lengths;
    [SerializeField, ReadOnly]
    private float[] percentages;
    [SerializeField, ReadOnly]
    private float[] routeTimes;
    // Start is called before the first frame update
    void Start()
    {
        totalTime = timer.duration;
        totalLength = route.CalculateLength(out lengths);
        routeTimes = new float[route.points.Length];
        for (int i = 0; i < lengths.Length; i++)
            routeTimes[i] = lengths[i] / totalLength * totalTime;
        timer.Update += Timer_Update;
        timer.Completed += Timer_Completed;
        timer.duration = routeTimes[current];
        ResetOffset();
    }
    public void ResetOffset()
    {
        offset = transform.position;
    }
    private void OnEnable()
    {
        timer.Start();
    }
    // Update is called once per frame
    void Update()
    {
        if (route.points.Length > 1)
            timer.Tick();
    }
    private void FixedUpdate()
    {
        velocity = (transform.position - previousPosition) / Time.fixedDeltaTime;
    }
    private void Timer_Update()
    {
        //if (current == route.GetNext(current))
        //    return;
        //transform.position = Vector3.SmoothDamp(transform.position,
        //    Vector3.Lerp(GetPoint(current), GetPoint(route.GetNext(current)), timer.normal),
        //    ref currentVelocity, smoothTime);

        transform.position = Vector3.Lerp(GetPoint(current), GetPoint(route.GetNext(current)), timer.normalized);
    }

    private Vector2 GetPoint(int index) => offset + route.points[index];
    private void Timer_Completed()
    {
 
        if (!route.loop && current == route.points.Length - 1 )
            return;
        route.MoveNext(ref _current);
        timer.duration = routeTimes[current];
        timer.Start();
    }





    private void OnCollisionStay2D(Collision2D collision)
    {
    }
}
