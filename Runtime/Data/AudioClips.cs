using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AudioClips : ScriptableObject
{
    public AudioClip[] clips;
    public AudioClip RandomElement() => clips.RandomElement();
}
