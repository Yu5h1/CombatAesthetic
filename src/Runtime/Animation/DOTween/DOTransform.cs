using DG.Tweening.Plugins.Options;
using UnityEngine;

public abstract class DOTransform<TValue, TPlugOptions> : TweenBehaviour<Transform, TValue, TPlugOptions>
    where TPlugOptions : struct, IPlugOptions
{
    public bool local = true;
}
