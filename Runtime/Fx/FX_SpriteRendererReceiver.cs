using System.Collections;
using UnityEngine;
using System.Linq;
using Yu5h1Lib;
using UnityEngine.Events;
using Yu5h1Lib.Game.Character;

[DisallowMultipleComponent]
public class FX_SpriteRendererReceiver : Fx_Receiver<Fx_SpriteRendererSender>
{
    private CharacterController2D controller;
    private SpriteRenderer[] spriteRenderers;
    private Material[] materials;
    private Timer timer;
    private Timer.Wait waiter;
    private AnimationCurve curve;
    private bool IsDepleted;

    [SerializeField]
    private UnityEvent Finish;

    private void Awake()
    {
        var spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        materials = new Material[spriteRenderers.Length];
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i] = spriteRenderers[i].material;
        }
        controller = GetComponent<CharacterController2D>();
    }
    private void Start()
    {
        timer = new Timer();
        waiter = timer.Waiting();
        timer.Update += Timer_Update;

    }
    private void Timer_Update(Timer t)
    {
        if (sender == null)
            return;
        SetAmount(curve.Evaluate(timer.normalized));
    }
    public void SetMaterialColor(Color color)
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
    public bool IsFinished;
    public override void Perform(Fx_SpriteRendererSender s)
    {
        if (IsFinished)
            return;
        sender = s;
        curve = sender.curve;
        UnityAction completeAction = null;
        if (controller.attribute.exhausted)
        {
            curve = sender.ExitCurve;
            completeAction = PerformExhausting;
            IsFinished = true;
        }
        if (!curve.keys.IsEmpty() && curve.keys.Length > 1)
            this.StartCoroutine(ref coroutine, PerformFx(completeAction));
    }
    IEnumerator PerformFx(UnityAction onComplete)
    {
        SetMaterialColor(sender.color);
        timer.duration = curve.keys.Last().time;
        timer.Start();
        yield return waiter;
        onComplete?.Invoke();
    }
    private void PerformExhausting()
    {
        var fx = PoolManager.Spawn<Transform>(sender.Fx_Exit, transform.position, transform.rotation);
        if (fx && fx.TryGetComponent(out ParticleSystem ps))
        {
            var main = ps.main;
            var shape = ps.shape;
            shape.meshShapeType = ParticleSystemMeshShapeType.Triangle;
            shape.shapeType = ParticleSystemShapeType.SpriteRenderer;
            shape.spriteRenderer = GetComponent<SpriteRenderer>();
        }
        gameObject.SetActive(false);
        Finish?.Invoke();
    }



}
