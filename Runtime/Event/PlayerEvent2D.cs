using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Yu5h1Lib;

public abstract class PlayerEvent2D : BaseEvent2D
{
    public readonly static TagOption EqualPlayerTagMask = new TagOption() { root = true, tag = "Player", mode = TagOption.FilterMode.Include };
    public static LayerMask CharacterLayer => LayerMask.GetMask("Character");

    public bool Validate(Collider2D other) => enabled && EqualPlayerTagMask.Compare(other.gameObject) && CharacterLayer.Contains(other.gameObject);
}
