using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEvent2D : MonoBehaviour
{
    public readonly static TagOption EqualPlayerTag = new TagOption() { tag = "Player", type = TagOption.ComparisionType.Equal };
    public static LayerMask CharacterLayer => 1 << LayerMask.NameToLayer("Character");

    public bool Validate(Collider2D other) => enabled && EqualPlayerTag.Compare(other.tag) && CharacterLayer.Contains(other.gameObject);

}
