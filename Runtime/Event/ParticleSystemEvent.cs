using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Yu5h1Lib;
using static UnityEngine.ParticleSystem;

[DisallowMultipleComponent]
public class ParticleSystemEvent : BaseParticleSystemBehaviour
{
    [SerializeField]
    private TagLayerMask filter;

    [SerializeField]
    private UnityEvent<Collider2D> _triggerEnter;
    [SerializeField]
    private UnityEvent<GameObject> _particleCollision;
    [SerializeField]
    private UnityEvent OnParticleSystemStoppedEvent;

    protected ParticleSystem.Particle[] particles;


    public ParticleSystem[] subParticleSystems { get; private set; }


    /// <summary>
    /// Unable to capture message
    /// </summary>
    //[SerializeField]
    //private UnityEvent<ParticleSystem.Particle> _ParticleBirth;
    //[SerializeField]
    //private UnityEvent<ParticleSystem.Particle> _ParticleDeath;

    protected override void OnInitializing()
    {
        base.OnInitializing();
        var main = particleSystem.main;
        main.stopAction = ParticleSystemStopAction.Callback;
        subParticleSystems = GetComponentsInChildren<ParticleSystem>();
    }

    private void Start()
    {
        
    }
    public void SetTriggerList2D(IEnumerable<Collider2D> targets)
    {
        if (filter.layers.value == 0)
            return;
        particleSystem.SetTriggerList2D(filter.Filter(targets));
    }

    //private void FixedUpdate()
    //{
    //if (!TriggerByCircleCast)
    //    return;

    //}
    private void OnParticleTrigger()
    {
        if (TryGetTriggerCollider(ParticleSystemTriggerEventType.Enter, out Particle particle, out Component component)
            && component is Collider2D collider && filter.Validate(this,collider))
            _triggerEnter?.Invoke(collider);
    }
    private void OnParticleTriggerEnter(GameObject gameObject)
    { 

    }
    
    protected void OnParticleSystemStopped()
    {
        if (!IsAvailable())
            return;
        OnParticleSystemStoppedEvent?.Invoke();
    }
    private void OnParticleCollision(GameObject other) {
        _particleCollision?.Invoke(other);
    }
    //private void OnParticleUpdateJobScheduled() {}
    public void DismissParticleOnTriggerEnter()
    {
        particleSystem.ModifyTriggerParticles(ParticleSystemTriggerEventType.Enter,DismissParticle);
    }
    public void DismissParticles()
    {
        var particles = new Particle[particleSystem.main.maxParticles];
        particleSystem.ModifyParticle(ref particles, DismissParticle);
    }
    public Particle DismissParticle(Particle source)
    {
        source.remainingLifetime = 0.01f;
        return source;
    }
    #region Enhance...
    public bool TryGetTriggerCollider(ParticleSystemTriggerEventType eventType,out Particle particle, out Component component)
    {
        component = null;
        particle = default(Particle);
        var particles = new List<Particle>();
        int numOfparticles = particleSystem.GetTriggerParticles(eventType, particles,out ColliderData data);
        for (int p = 0; p < numOfparticles; p++)
        {
            for (int c = 0; c < data.GetColliderCount(p); c++)
                if (component = data.GetCollider(p, c))
                {
                    particle = particles[p];
                    return true;
                }
        }
        return false;
    }
    private void OnDisable()
    {
        particleSystem.ClearTriggerList();
    }
    #endregion
}
