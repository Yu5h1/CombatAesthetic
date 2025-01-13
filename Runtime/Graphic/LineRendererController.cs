using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yu5h1Lib;


public class LineRendererController : ComponentController<LineRenderer>
{
    public LineRenderer lineRenderer => component;
    public bool loop;

    public int PositionCount => loop && targets.Count > 2 ? targets.Count + 1: targets.Count;

    [SerializeField]
    private List<Transform> _targets;
    public List<Transform> targets
    {
        get => _targets;
        set 
        {
            if (targets.SequenceEqual(value))
                return;
            _targets = value;
        }
    }
    public Material lineMaterial => lineRenderer.sharedMaterial;
    public float scrollSpeed = 0.5f;

    private void FixedUpdate()
    {
        
        Refresh();
    }
    [ContextMenu(nameof(Refresh))]
    public void Refresh()
    {
        if (targets.IsEmpty() || targets.Count == 1)
            return;
        if (lineRenderer.positionCount != PositionCount)
            lineRenderer.positionCount = PositionCount;
        if (lineMaterial)
        {
            var offset = lineMaterial.GetTextureOffset("_BaseMap");
            offset.x += scrollSpeed * Time.fixedDeltaTime;
            offset.x = Mathf.Repeat(offset.x, 1);
            lineMaterial.SetTextureOffset("_BaseMap", offset);
        }
        for (int i = 0; i < targets.Count; i++)
        {
            if (lineRenderer.GetPosition(i) != targets[i].position)
            {
                lineRenderer.SetPosition(i, targets[i].position);
            }
        }
        if (loop && targets.Count > 2)
        {
            if (lineRenderer.GetPosition(lineRenderer.positionCount - 1 ) != targets[0].position)
                lineRenderer.SetPosition(lineRenderer.positionCount - 1, targets[0].position);
        }           
    }
}
