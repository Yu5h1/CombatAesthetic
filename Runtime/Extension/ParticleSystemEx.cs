using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Yu5h1Lib;
using static UnityEngine.ParticleSystem;

public static class ParticleSystemEx
{
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
    public static void SetTriggerList2D<T>(this ParticleSystem particleSystem, IEnumerable<T> targets) where T : Component
    {
        var triggerModule = particleSystem.trigger;
        triggerModule.enabled = true;
        particleSystem.ClearTriggerList();
        foreach (var target in targets)
            triggerModule.AddCollider(target);
    }

    public static Component[] GetTriggerList(this ParticleSystem particleSystem)
    {
        var triggerModule = particleSystem.trigger;
        var results = new Component[triggerModule.colliderCount];
        for (int i = 0; i < results.Length; i++)
            results[i] = triggerModule.GetCollider(i);
        return results;
    }
    public static void ClearTriggerList(this ParticleSystem particleSystem)
    {
        var triggerModule = particleSystem.trigger;
        var targets = particleSystem.GetTriggerList();
        foreach ( var target in targets )
            triggerModule.RemoveCollider(target);
    }
}