using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fx_SpriteRendererSender : Fx_Sender
{
    public Color color = Color.white;
    public AnimationCurve curve;
    public AnimationCurve ExitCurve;
    public string Fx_Exit = "FragmentExplod(Sprite)";
    public override void Perform(Collider2D target)
    {
        if (target.TryGetComponent(out FX_SpriteRendererReceiver receiver))
            receiver.Perform(this);
    }    
}
