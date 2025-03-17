using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yu5h1Lib;


public class AudioClips : ScriptableObject
{
    public AudioClip[] clips;
    public AudioClip RandomElement() => clips.RandomElement();
}
