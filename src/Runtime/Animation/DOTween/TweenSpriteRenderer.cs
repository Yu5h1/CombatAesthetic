using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;

[RequireComponent(typeof(SpriteRenderer))]
public class TweenSpriteRenderer : TweenColorRenderer<SpriteRenderer>
{
    protected override TweenerCore<Color, Color, ColorOptions> CreateTweenCore()
        => component.DOColor(_endValue, Duration);
}
