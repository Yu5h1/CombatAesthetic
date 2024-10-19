using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yu5h1Lib.Game;
using DG.Tweening;
using System.Linq;

public class FX_SpriteRendererReceiver : Fx_Receiver<Fx_SpriteRendererSender>
{
    private AttributeBehaviour attributeBehaviour;
    private SpriteRenderer[] spriteRenderers;
    private Material[] materials;
    private Timer timer;
    private AnimationCurve curve;
    private bool IsDepleted;

    private void Awake()
    {
        var spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        materials = new Material[spriteRenderers.Length];
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i] = spriteRenderers[i].material;
        }
        attributeBehaviour = GetComponent<AttributeBehaviour>();
    }
    private void Start()
    {
        timer = new Timer();
        timer.Update += Timer_Update;

    }
    private void Timer_Update()
    {
        if (sender == null)
            return;
        SetAmount(curve.Evaluate(timer.normal));
    }
    public void SetColor(Color color)
    {
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i].SetColor("_Color", color);
        }
    }
    public void SetAmount(float amount)
    {
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i].SetFloat("_Amount", amount);
        }
    }
    Coroutine coroutine;
    public override void Perform(Fx_SpriteRendererSender s)
    {
        if (coroutine != null)
            StopCoroutine(coroutine);
        sender = s;
        IsDepleted = attributeBehaviour.TryGetState(AttributeType.Health, out AttributeStat stat) && stat.IsDepleted;
        curve = IsDepleted ? sender.ExitCurve : sender.curve;
        coroutine = StartCoroutine(PerformFx());
    }
    IEnumerator PerformFx()
    {
        SetColor(sender.color);
        timer.duration = curve.keys.Last().time;
        yield return new Timer.Wait<Timer>(timer);
        if (IsDepleted)
        {
            var fx = PoolManager.instance.Spawn<Transform>(sender.Fx_Exit, transform.position, transform.rotation);
            if (fx && fx.TryGetComponent(out ParticleSystem ps)) {
                var main = ps.main;
                var shape = ps.shape;
                shape.meshShapeType = ParticleSystemMeshShapeType.Triangle;
                shape.shapeType = ParticleSystemShapeType.SpriteRenderer;
                shape.spriteRenderer = GetComponent<SpriteRenderer>();
            }
            gameObject.SetActive(false);
        }
    }
}
