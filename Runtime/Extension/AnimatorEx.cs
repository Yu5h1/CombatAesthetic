using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class AnimatorEx 
{
    public static int GetDominantLayer(this Animator animator)
    {
        int maxLayerIndex = 1;
        float maxWeight = 0f;

        for (int i = 1; i < animator.layerCount; i++)
        {
            float layerWeight = animator.GetLayerWeight(i);
            if (layerWeight > maxWeight)
            {
                maxWeight = layerWeight;
                maxLayerIndex = i;
            }
        }
        return maxWeight > 0 ? maxLayerIndex : 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasState(this Animator anim, string stateName, int layer = 0)
    {
        int stateID = Animator.StringToHash(stateName);
        return anim.HasState(layer, stateID);

    }
}
