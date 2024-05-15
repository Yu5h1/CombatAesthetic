using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using FloatTweener = DG.Tweening.Core.TweenerCore<float, float, DG.Tweening.Plugins.Options.FloatOptions>;

[RequireComponent(typeof(CanvasGroup))]
public class DOFade : TweenBehaviour<CanvasGroup,float,FloatOptions>
{
    protected override FloatTweener CreateTweenCore() => component.DOFade(endValue, Duration);
}
