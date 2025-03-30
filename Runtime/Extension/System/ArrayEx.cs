using System.Collections.Generic;
using UnityEngine;

public static class ArrayEx
{
    public static int ShiftIndex<T>(this IList<T> array, int index, bool next)
    {
        var result = index + (next ? 1 : -1);
        result = result < 0 ? array.Count - 1 : (result >= array.Count ? 0 : result);
        return result;
    }
    public static T GetWeightedRandom<T>(this IList<T> items, int[] weights)
    {
        int totalWeight = 0;
        foreach (int weight in weights)
        {
            totalWeight += weight;
        }

        int randomValue = Random.Range(0, totalWeight);
        int cumulativeWeight = 0;

        for (int i = 0; i < items.Count; i++)
        {
            cumulativeWeight += weights[i];
            if (randomValue < cumulativeWeight)
            {
                return items[i];
            }
        }
        return items[items.Count - 1]; 
    }
}