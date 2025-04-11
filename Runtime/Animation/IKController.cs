using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.U2D.IK;

using Yu5h1Lib;

public class IKController : BaseMonoBehaviour
{
    [SerializeField,ReadOnly]
    private IKManager2D _ikManager;
    public IKManager2D ikManager { get => _ikManager; set => _ikManager = value; }

    public Transform root;
    public Transform target;
    public float distance;
    public Vector2 offset;
    public Vector2 direction = Vector2.up;
    public bool NoUpdate;

    protected override void OnInitializing()
    {
        this.GetComponent(ref _ikManager);
    }

    private void Start() { }

    void Update()
    {
        if (!isActiveAndEnabled || !root || !target || NoUpdate)
            return;
        Refresh();
    }

    public void Refresh()
    {
        target.position = (Vector2)root.position + offset + direction * distance;
    }
    private void OnDisable()
    {
        RestoreDefaultPose();
    }

    [ContextMenu(nameof(RestoreDefaultPose))]
    public void RestoreDefaultPose()
    {
        foreach (var solver in ikManager.solvers)
        {
            for (int i = 0; i < solver.chainCount; ++i)
            {
                var chain = solver.GetChain(i);
                chain.RestoreDefaultPose(solver.constrainRotation);

                if (chain.target)
                {
                    chain.target.position = chain.effector.position;
                    chain.target.rotation = chain.effector.rotation;
                }
            }
        }
    }

}