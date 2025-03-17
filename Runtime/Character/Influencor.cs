using System.Collections;
using UnityEngine;
using Yu5h1Lib;
using Yu5h1Lib.Game.Character;

public class Influencor : BaseMonoBehaviour
{
    [SerializeField]
    public AffectType affectType;
    [SerializeField]
    private EnergyInfo info;


//#pragma warning disable 0109 
//    private new Rigidbody2D rigidbody;
//#pragma warning restore 0109 
    protected override void OnInitializing()
    {

    }

    //public void Affect(Collider2D other, Vector2 strength)
    //{
    //    if (!isActiveAndEnabled)
    //        return;
    //    if (other.TryGetComponentInParent(out AnimatorCharacterController2D controller,true))
    //        controller.HitFrom(strength,false,false);
    //    if (other.TryGetComponentInParent(out AttributeBehaviour stat,true))
    //        stat.Affect(affectType, info);
    //}
    public void Affect(Collider2D other)
    {
        if (!isActiveAndEnabled)
            return;
        if (other.TryGetComponentInParent(out AttributeBehaviour stat,true))
            stat.Affect(affectType, info);
    }
    public void Hit(Collider2D other)
    {
        if (!isActiveAndEnabled)
            return;
        if (other.TryGetComponentInParent(out AnimatorCharacterController2D controller, true))
        {
            if (TryGetComponent(out ImpactForce2D impactor)){
                controller.HitFrom(impactor.force, true, false);
            }else
                controller.HitFrom(TryGetComponent(out Rigidbody2D rigidbody) ? rigidbody.velocity : -controller.velocity,false , true);
        }
            
    }
    public void RecoilHit()
    {
        if (!isActiveAndEnabled)
            return;
        if (this.TryGetComponentInParent(out CharacterController2D controller, true))
            controller.HitFrom(-controller.velocity,true,true);
    }
    public void RecoilHit(float multiplier)
    {
        if (!isActiveAndEnabled)
            return;
        if (this.TryGetComponentInParent(out CharacterController2D controller, true))
            controller.HitFrom(-controller.velocity * multiplier, false, true);
    }
    public void AddForce(Collider2D other)
    {
        if (!isActiveAndEnabled)
            return;
        if (other.TryGetComponentInParent(out AnimatorCharacterController2D controller, true))
            controller.AddForce(transform.up * info.amount);
        else if (other.TryGetComponentInParent(out Rigidbody2D rigidbody, true))
            rigidbody.AddForce(transform.up * info.amount);
    }
}
