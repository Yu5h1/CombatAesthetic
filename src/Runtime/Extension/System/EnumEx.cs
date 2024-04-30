using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public static class EnumEx
{
    public static IEnumerable<T> SeparateFlags<T>(this T flags) where T : Enum
    {
        if (!Attribute.IsDefined(typeof(T), typeof(FlagsAttribute)))
            throw new ArgumentException("Flags attribute is required for the specified enum type.");

        Array enumValues = Enum.GetValues(flags.GetType());
        foreach (Enum value in enumValues)
        {
            int flagValue = Convert.ToInt32(value);
            if (flagValue != 0 && flags.HasFlag(value))
                yield return (T)Convert.ChangeType(value, typeof(T));
        }
    }
    public static T[] GetValues<T>() where T : Enum => Enum.GetValues(typeof(T)).Cast<T>().ToArray();
}