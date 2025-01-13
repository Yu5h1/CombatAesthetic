using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
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
}