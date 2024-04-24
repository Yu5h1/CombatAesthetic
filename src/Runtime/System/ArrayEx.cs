using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class ArrayEx
{
    #region Swap
    public static void Swap<T>(this IList<T> array, int from, int to)
    {
        if (array.IsEmpty() || !array.Validate(from) || !array.Validate(to) || from == to)
            return;
        (array[from], array[to]) = (array[to], array[from]);
    }
    public static void Swap<T>(this IList<T> array, T item, int to) => array.Swap(array.IndexOf(item), to);

    public static void SwapShift<T>(this IList<T> array, T item, bool next)
    {
        var from = array.IndexOf(item);
        var to = from + (next ? 1 : -1);
        to = to < 0 ? array.Count - 1 : (to >= array.Count ? 0 : to);
        array.Swap(from, to);
    }
    #endregion

    public static int IndexOf<T>(this T[] array,T value) => array.IsEmpty() ? -1 : Array.IndexOf(array, value);
	public static bool Validate<T>(this IList<T> list, int index)
		=> list == null ? false : index >= 0 && index < list.Count;
    public static bool IsEmpty<T>(this IEnumerable<T> enumerable)
        => enumerable == null || !enumerable.Any();
    public static bool TryFind<T>(this IEnumerable<T> items,Predicate<T> predicate,out T result)
    {
        result = default(T);
        foreach (var item in items)
        {
            if (predicate(item))
            {
                result = item;
                return true;
            }
        }
        return false;
    }
            
}