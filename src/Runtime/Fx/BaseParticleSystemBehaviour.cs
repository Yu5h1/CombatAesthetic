using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

[RequireComponent(typeof(ParticleSystem))]
public abstract class BaseParticleSystemBehaviour : MonoBehaviourEnhance
{
    protected new ParticleSystem particleSystem;
    public float normalizedTime => particleSystem.time.GetNormal(particleSystem.main.duration);
    protected virtual void OnEnable()
    {
        particleSystem = particleSystem ?? GetComponent<ParticleSystem>();
    }
}
