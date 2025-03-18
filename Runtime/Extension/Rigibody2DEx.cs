using UnityEngine;

namespace Yu5h1Lib
{
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never), System.ComponentModel.Browsable(false)]
    public static class Rigibody2DEx
    {
        public static void Reset(this Rigidbody2D rb, Vector2? pos = null, float? rot = null)
        {
            rb.velocity = Vector2.zero;          
            rb.angularVelocity = 0f;             
            rb.simulated = false;                
            rb.simulated = true;                 
            if (pos != null)
                rb.position = pos.Value;
            if (rot != null)
                rb.rotation = rot.Value;
        }
    } 
}
