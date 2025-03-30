using UnityEngine;
using Yu5h1Lib;

[RequireComponent(typeof(ParticleSystem))]
public abstract class BaseParticleSystemBehaviour : BaseMonoBehaviour
{
    [SerializeField,ReadOnly]
    private ParticleSystem _particleSystem;
#pragma warning disable 0109
    public new ParticleSystem particleSystem{ get => _particleSystem; protected set => _particleSystem = value; }
#pragma warning restore 0109
    public float normalizedTime => particleSystem.time.GetNormal(particleSystem.main.duration);

    protected override void OnInitializing()
    {
        this.GetComponent(ref _particleSystem);
    }
}
