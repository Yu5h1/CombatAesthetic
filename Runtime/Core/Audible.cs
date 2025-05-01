using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yu5h1Lib;

public class Audible : MonoBehaviour
{
    public float delay;
    void Start() {}
    public void PlayAudioClip(AudioClip clip)
    {
        if (!this.IsAvailable())
            return;
        if ($"{name} tring to play Null AudioClip".printErrorIf(!clip))
            return;
        AudioManager.Play(clip, transform.position);
    }
}
