using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.ParticleSystem;

[DisallowMultipleComponent]
public class ParticleSystemEvent : BaseParticleSystemBehaviour
{
    //public bool TriggerByCircleCast;
    public UnityEvent ParticleTriggerEnter;
    public UnityEvent ParticleCollision;
    public UnityEvent OnParticleSystemStoppedEvent;
    public UnityEvent<Transform,Vector2> ParticleTriggerEnterCollider;
    protected ParticleSystem.Particle[] particles;
    protected override void OnEnable()
    {
        base.OnEnable();
        var main = particleSystem.main;
        main.stopAction = ParticleSystemStopAction.Callback;
    }
    //private void FixedUpdate()
    //{
        //if (!TriggerByCircleCast)
        //    return;

    //}
    private void OnParticleTrigger()
    {        
        if (TryGetTriggerCollider(ParticleSystemTriggerEventType.Enter, out Particle particle, out Component component))
            ParticleTriggerEnterCollider?.Invoke(component.transform, (Vector2)(particle.velocity * particle.GetCurrentSize(particleSystem)));
        ParticleTriggerEnter?.Invoke();
    }
    private void OnParticleTriggerEnter(GameObject gameObject)
    { 

    }
    protected void OnParticleSystemStopped()
    {
        OnParticleSystemStoppedEvent?.Invoke();
    }
    //private void OnParticleSystemStopped();
    private void OnParticleCollision(GameObject other) {
        ParticleCollision?.Invoke();
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
    #endregion
}
