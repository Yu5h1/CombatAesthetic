using System.Collections;
using UnityEngine;
using Yu5h1Lib.Game.Character;

public class TriggerAffectCharacterEvent2D : TriggerCharacterEvent2D
{
    public float duration = 3;
    public AffectType affectType;
    public EnergyInfo data;
    protected override bool OnTriggerEntered2D(Controller2D character)
    {
        if (!base.OnTriggerEntered2D(character))
            return false;
        if (character.TryGetComponent(out AttributeStatBehaviour attributeStatus))
            attributeStatus.Affect(affectType,data);
        return true;
    }
    public void BoostMultiplier(Controller2D character)
    {
        character.StopCoroutine(AffectForSeconds(character));
        character.StartCoroutine(AffectForSeconds(character));
    }
    private IEnumerator AffectForSeconds(Controller2D character)
    {
        character.BoostMultiplier = 2;
        yield return new WaitForSeconds(duration);
        character.BoostMultiplier = 1;
    }
}
