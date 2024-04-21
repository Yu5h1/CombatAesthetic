using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public static class ParticleSystemEx
{
	public static void ModifyTriggerParticles(this ParticleSystem particleSystem, ParticleSystemTriggerEventType eventType, Func<Particle, Particle> modifier)
	{
        var particles = new List<Particle>();
        int numOfparticles = particleSystem.GetTriggerParticles(eventType, particles);
        for (int i = 0; i < numOfparticles; i++)
            particles[i] = modifier(particles[i]);
        particleSystem.SetTriggerParticles(eventType, particles);
    }
    public static void ModifyParticle(this ParticleSystem particleSystem,ref Particle[] particles,Func<Particle,Particle> modifier)
    {
        if (particles.Length < particleSystem.main.maxParticles)
        {
            Debug.LogWarning($"particles length need to be same as maxParticles");
            return;
        }
        int numParticlesAlive = particleSystem.GetParticles(particles);
        for (int i = 0; i < numParticlesAlive; i++)
            particles[i] = modifier(particles[i]);
        particleSystem.SetParticles(particles, numParticlesAlive);
    }
    public static bool TryOverlapCircle(this ParticleSystem particleSystem, IList<Particle> particles, int numOfparticles, out Collider2D collider)
    {
        collider = null;
        for (int i = 0; i < numOfparticles; i++)
        {
            var result = Physics2D.OverlapCircle((Vector2)particles[i].position, particles[i].GetCurrentSize(particleSystem));
            if (result)
            {
                collider = result;
                return true;
            }
        }
        return false;
    }
}