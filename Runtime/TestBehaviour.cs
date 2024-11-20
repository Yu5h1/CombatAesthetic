using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yu5h1Lib;

public class TestBehaviour : MonoBehaviour
{
    [ShowInInspector]
    public string[] OK;
    public int num;

    public Vector2 v;

    public Line2D line = new Line2D(Vector2.zero,Vector2.up);
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    [ContextMenu("PrintHideFlags")]
    public void PrintHideFlags()
    {
        gameObject.hideFlags.print();
    }

    [ContextMenu("Test")]
    public void Test()
    {
    }

}
