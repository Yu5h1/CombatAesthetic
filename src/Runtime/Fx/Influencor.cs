using System.Collections;
using UnityEngine;
using Yu5h1Lib.Game.Character;

public class Influencor : MonoBehaviour
{
    [SerializeField]
    public AffectType affectType;
    [SerializeField]
    private EnergyInfo info;

    public void Affect(Collider2D other, Vector2 strength)
    {
        if (other.TryGetComponent(out Controller2D controller))
            controller.Hit(strength);
        if (other.TryGetComponent(out AttributeStatBehaviour stat))
            stat.Affect(affectType, info);
    }
    public void Affect(Collider2D other)
    {
        if (other.TryGetComponent(out AttributeStatBehaviour stat))
            stat.Affect(affectType, info);
        if (other.TryGetComponent(out Controller2D controller))
            controller.Hit( TryGetComponent(out Rigidbody2D rigidbody) ? rigidbody.velocity : Vector2.one * info.amount);
    }


    public void BoostMultiplier(Collider2D target)
    {
        if (!target.gameObject.TryGetComponent(out Controller2D character))
            return;
        character.StopCoroutine(AffectForSeconds(character));
        character.StartCoroutine(AffectForSeconds(character));
    }
    private IEnumerator AffectForSeconds(Controller2D character)
    {
        character.BoostMultiplier += 1;
        yield return new WaitForSeconds(3);
        if (character.BoostMultiplier > 1)
            character.BoostMultiplier -= 1;
    }
}
