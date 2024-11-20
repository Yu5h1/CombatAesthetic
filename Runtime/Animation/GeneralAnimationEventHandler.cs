using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yu5h1Lib;

public class GeneralEventHandler : MonoBehaviour
{
    public void PlayAudio(string audioName)
    {
        if (ResourcesEx.TryLoad($"Sound/{audioName}", out AudioClip clip))
            GameManager.instance.PlayAudio(clip, 0.5f);
        else
        {
            var clipName = clip == null ? $"footstep{audioName}.mp3 not found" : clip.name;
            $"PlayAudio : {clipName} does not Exists.".printWarning();
        }
    }
}
