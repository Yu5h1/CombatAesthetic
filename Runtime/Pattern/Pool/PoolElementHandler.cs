using UnityEngine;
using UnityEngine.Events;
using Yu5h1Lib;

public class PoolElementHandler : MonoBehaviour
{
    [SerializeField,ReadOnly]
    public Component element;

    [SerializeField]
    private UnityEvent despawned;
    private void Start() {}

    public void Despawn() {
        if (!isActiveAndEnabled)
            return;
        if ($"{name} elementKey was undefined.".printErrorIf(element == null))
            return;
        PoolManager.Despawn(element);
        despawned?.Invoke();
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
