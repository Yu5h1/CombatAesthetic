using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class ArrayEx
{
    public static int ShiftIndex<T>(this IList<T> array, int index, bool next)
    {
        var result = index + (next ? 1 : -1);
        result = result < 0 ? array.Count - 1 : (result >= array.Count ? 0 : result);
        return result;
    }
}