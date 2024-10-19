using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

public class DOMove2D : DOTransform<Vector3,VectorOptions>
{
    public Vector2 velocity { get; private set; }
    public float ValueMultiplier = 1;

    protected override void Start() {
        base.Start();
        velocity = (endValue - (local ? transform.localPosition : transform.position)) / Duration;
        if (local)
            velocity = transform.up * velocity.y + transform.right * velocity.x;
    }
    protected override TweenerCore<Vector3, Vector3, VectorOptions> CreateTweenCore()
        => local ? transform.DOLocalMove(endValue, Duration) :
                   transform.DOMove(endValue, Duration);

    protected override void OnComplete() => OnComplete(velocity * ValueMultiplier);
    protected override void OnRewind() => OnRewind( -velocity * ValueMultiplier);

    [ContextMenu("Set End Value From Position")]
    public void SetEndValueFromPosition()
    {
        _endValue = local ? transform.localPosition : transform.position;
    }

    public override string ToString() => tweener.ToString();

}