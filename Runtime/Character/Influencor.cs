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


    private void Start() {}

    protected override void OnInitializing() { }

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
        if (!IsAvailable())
            return;
        if (other.TryGetComponentInParent(out AttributeBehaviour stat,true))
            stat.Affect(affectType, info);
    }
    public void Hit(Collider2D other)
    {
        if (!IsAvailable())
            return;
        if (other.TryGetComponentInParent(out AnimatorCharacterController2D controller, true))
        {
            if (TryGetComponent(out ImpactForce2D impactor)){
                controller.HitFrom(impactor.force, true, false);
            }else
                controller.HitFrom(TryGetComponent(out Rigidbody2D rigidbody) ? rigidbody.velocity : -controller.velocity,false , true);
        }
    }
    public void Affect(CharacterController2D other)
    {
        if (!IsAvailable() || !other.IsAvailable() || other.IsInvincible)
            return;
        other.attribute.Affect(affectType, info);
    }

    public void RecoilHit() => RecoilHit(1);
    public void RecoilHit(float multiplier)
    {
        if (!IsAvailable())
            return;
        if (this.TryGetComponentInParent(out CharacterController2D controller, true))
            controller.HitFrom(-controller.velocity * multiplier,true, false);
    }
    public void AddForce(Collider2D other)
    {
        if (!IsAvailable())
            return;
        if (other.TryGetComponentInParent(out AnimatorCharacterController2D controller, true))
            controller.AddForce(transform.up * info.amount);
        else if (other.TryGetComponentInParent(out Rigidbody2D rigidbody, true))
            rigidbody.AddForce(transform.up * info.amount);
    }
}
