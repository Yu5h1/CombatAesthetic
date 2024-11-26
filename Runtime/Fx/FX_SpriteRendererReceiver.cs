using System.Collections;
using UnityEngine;
using System.Linq;
using Yu5h1Lib;
using UnityEngine.Events;
using Yu5h1Lib.Game.Character;

[DisallowMultipleComponent]
public class FX_SpriteRendererReceiver : Fx_Receiver<Fx_SpriteRendererSender>
{
    private AttributeBehaviour attributeBehaviour;
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
        attributeBehaviour = GetComponent<AttributeBehaviour>();
    }
    private void Start()
    {
        timer = new Timer();
        waiter = timer.Waiting();
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
    public bool IsFinished;
    public override void Perform(Fx_SpriteRendererSender s)
    {
        if (IsFinished)
            return;
        sender = s;
        IsDepleted = attributeBehaviour.TryGetState(AttributeType.Health, out AttributeStat stat) && stat.IsDepleted;
        curve = IsDepleted ? sender.ExitCurve : sender.curve;
        if (IsDepleted)
            IsFinished = true;
        if (!curve.keys.IsEmpty() && curve.keys.Length > 1)
            this.StartCoroutine(ref coroutine, PerformFx());
    }
    IEnumerator PerformFx()
    {
        SetColor(sender.color);
        timer.duration = curve.keys.Last().time;
        timer.Start();
        yield return waiter;
        if (IsDepleted && sender)
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
            Finish?.Invoke();

        }
    }
}
