using System.Collections;
using UnityEngine;
using Yu5h1Lib;
using Yu5h1Lib.Game.Character;

public class Influencor : MonoBehaviour
{
    [SerializeField]
    public AffectType affectType;
    [SerializeField]
    private EnergyInfo info;

    private void Start()
    {
            
    }

    public void Affect(Collider2D other, Vector2 strength)
    {
        if (!isActiveAndEnabled)
            return;
        if (other.TryGetComponentInParent(out AnimatorController2D controller,true))
            controller.HitFrom(strength);
        if (other.TryGetComponentInParent(out AttributeBehaviour stat,true))
            stat.Affect(affectType, info);
    }
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
        if (other.TryGetComponentInParent(out AnimatorController2D controller, true))
            controller.HitFrom(TryGetComponent(out Rigidbody2D rigidbody) ? rigidbody.velocity : -controller.velocity);
    }
    public void RecoilHit()
    {
        if (!isActiveAndEnabled)
            return;
        if (this.TryGetComponentInParent(out Controller2D controller, true))
            controller.HitFrom(-controller.velocity);
    }
    public void AddForce(Collider2D other)
    {
        if (!isActiveAndEnabled)
            return;
        if (other.TryGetComponentInParent(out AnimatorController2D controller, true))
            controller.AddForce(transform.up * info.amount);
        else if (other.TryGetComponentInParent(out Rigidbody2D rigidbody, true))
            rigidbody.AddForce(transform.up * info.amount);
    }
}
