using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestBehaviour : MonoBehaviour
{
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
        gameObject.hideFlags.Log();
    }
}
