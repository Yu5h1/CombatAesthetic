using System;
using Unity.Collections;
using UnityEngine;

public class ParticleTracor : BaseParticleSystemBehaviour
{
    public Transform target;
    [Range(0.1f, 1f)]
    public float redirectRate = 0.1f;

    #region parameters
    protected ParticleSystem.Particle[] particles;
    [ReadOnly]
    public Vector3[] SmoothDamps;
    //private UpdateParticlesJob job = new UpdateParticlesJob();
    #endregion
    protected void OnEnable()
    {
        particles = new ParticleSystem.Particle[particleSystem.main.maxParticles];
        SmoothDamps = new Vector3[particleSystem.main.maxParticles];
        //job.SmoothDamps = new NativeArray<Vector3>(SmoothDamps, Allocator.TempJob);

    }
    protected void OnDisable()
    {
        //job.SmoothDamps.Dispose();
    }
    protected void LateUpdate()
    {
        if (particleSystem == null || target == null)
            return;
        //job.target = target.transform.position;

        #region Old Method
        int numParticlesAlive = particleSystem.GetParticles(particles);
        for (int i = 0; i < numParticlesAlive; i++)
        {
            var dir = (target.transform.position - particles[i].position).normalized;

            dir = Vector3.SmoothDamp(particles[i].velocity.normalized, dir, ref SmoothDamps[i], redirectRate);

            particles[i].velocity = dir * particleSystem.main.startSpeed.Evaluate(0);
        }
        particleSystem.SetParticles(particles, numParticlesAlive);
        #endregion
    }
    //private void OnParticleUpdateJobScheduled()
    //{
    //    job.Schedule(particleSystem).Complete();
    //}
    //struct UpdateParticlesJob : IJobParticleSystem
    //{
    //    [ReadOnly]
    //    public Vector3 target;
        
    //    public NativeArray<Vector3> SmoothDamps;
    //    public void Execute(ParticleSystemJobData particles)
    //    {
    //        var PosX = particles.positions.x;
    //        var PosY = particles.positions.y;
    //        var PosZ = particles.positions.z;
    //        var vX = particles.velocities.x;
    //        var vY = particles.velocities.y;
    //        var vZ = particles.velocities.z;
            

    //        for (int i = 0; i < particles.count; i++)
    //        {
    //            var pos = new Vector3(PosX[i], PosY[i], PosZ[i]);
    //            var v = new Vector3(vX[i], vY[i], vZ[i]);
        
    //        }
    //    }
    //}
}
