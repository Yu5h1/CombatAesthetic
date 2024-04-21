using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yu5h1Lib.Game.Character;

public class Influencor : MonoBehaviour
{
    [SerializeField]
    public AffectType affectType;
    [SerializeField]
    private EnergyInfo info;

    public void Affect(Transform target, Vector2 strength)
    {
        if (target.TryGetComponent(out Controller2D controller))
            controller.Push(strength);
        if (target.TryGetComponent(out AttributeStatBehaviour stat))
            stat.Affect(affectType, info);
    }
    public void Affect(Collider2D target)
    {
        if (target.TryGetComponent(out AttributeStatBehaviour stat))
            stat.Affect(affectType, info);
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
        character.BoostMultiplier = 2;
        yield return new WaitForSeconds(3);
        character.BoostMultiplier = 1;
    }
}
