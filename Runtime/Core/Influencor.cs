using System.Collections;
using UnityEngine;
using Yu5h1Lib;
using Yu5h1Lib.Game.Character;

public class Influencor : MonoBehaviourEnhance
{
    [SerializeField]
    public AffectType affectType;
    [SerializeField]
    private EnergyInfo info;

    public void Affect(Collider2D other, Vector2 strength)
    {
        if (other.TryGetComponentInParent(out AnimatorController2D controller,true))
            controller.Hit(strength);
        if (other.TryGetComponentInParent(out AttributeBehaviour stat,true))
            stat.Affect(affectType, info);
    }
    public void Affect(Collider2D other)
    {
        if (other.TryGetComponentInParent(out AttributeBehaviour stat,true))
            stat.Affect(affectType, info);
    }
    public void Hit(Collider2D other)
    {
        if (other.TryGetComponentInParent(out AnimatorController2D controller, true))
            controller.Hit(TryGetComponent(out Rigidbody2D rigidbody) ? rigidbody.velocity : Vector2.one * info.amount);
    }
    public void AddForce(Collider2D other)
    {
        if (other.TryGetComponentInParent(out AnimatorController2D controller, true))
            controller.AddForce(transform.up * info.amount);
        else if (other.TryGetComponentInParent(out Rigidbody2D rigidbody, true))
            rigidbody.AddForce(transform.up * info.amount);
    }
}
