using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yu5h1Lib.Game.Character;

namespace Yu5h1Lib.Game
{
    public class FX_Boost : Fx_Sender
    {
        public float duration = 1;

        public override void Perform(Collider2D target)
        {
            if (!target.gameObject.TryGetComponent(out Controller2D character))
                return;
            character.StopCoroutine(AffectForSeconds(character));
            character.StartCoroutine(AffectForSeconds(character));
        }
        private IEnumerator AffectForSeconds(Controller2D character)
        {
            character.BoostMultiplier += 1;
            yield return new WaitForSeconds(duration);
            if (character.BoostMultiplier > 1)
                character.BoostMultiplier -= 1;
        }
    }
}