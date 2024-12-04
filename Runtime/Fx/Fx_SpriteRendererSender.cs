using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Fx_SpriteRendererSender : Fx_Sender
{
    public Color color = Color.white;
    public AnimationCurve curve;
    public AnimationCurve ExitCurve;
    public string Fx_Exit;
    public override void Perform(Collider2D target)
    {
        if (target.transform.root.TryGetComponent(out FX_SpriteRendererReceiver receiver) && receiver.enabled)
            receiver.Perform(this);
    }    
}
