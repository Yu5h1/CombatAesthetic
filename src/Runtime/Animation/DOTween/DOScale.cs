using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

public class DOScale : TweenBehaviour<Transform,Vector3,VectorOptions>
{
    protected override TweenerCore<Vector3, Vector3, VectorOptions> CreateTweenCore()
        => transform.DOScale(endValue, Duration);

}
