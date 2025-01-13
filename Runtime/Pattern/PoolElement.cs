using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yu5h1Lib;

public class PoolElement : MonoBehaviour
{
    public ComponentPool.Map map { get; internal set; }
    public void Despawn() {
        if (map == null)
            return;
        PoolManager.Despawn(map);
    }

    public void DespawnMesh(ParticleSystem particleSystem)
    {
        if (particleSystem.shape.mesh == null)
            return;
        var shapeModule = particleSystem.shape;
        MeshPool.Release(shapeModule.mesh);
        shapeModule.mesh = null;
    }
}
