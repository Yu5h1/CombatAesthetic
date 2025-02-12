using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yu5h1Lib;
using static UnityEngine.ParticleSystem;

[RequireComponent(typeof(ParticleSystem))]
public abstract class BaseParticleSystemBehaviour : BaseEvent2D
{
#pragma warning disable 0109
    protected new ParticleSystem particleSystem;
#pragma warning restore 0109
    public float normalizedTime => particleSystem.time.GetNormal(particleSystem.main.duration);
    protected virtual void OnEnable()
    {
        particleSystem = particleSystem ?? GetComponent<ParticleSystem>();
    }
}
