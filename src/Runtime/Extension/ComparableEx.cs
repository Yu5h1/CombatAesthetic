using System;

namespace Yu5h1Lib
{
    public static class ComparableEx
    {        
        public static T Clamp<T>(this T v, T min, T max) where T : IComparable
            => v.LimitMin(min).LimitMax(max);
        public static T LimitMax<T>(this T v, T max) where T : IComparable
            => v.CompareTo(max) >= 0 ? max : v;
        public static T LimitMin<T>(this T v, T min) where T : IComparable
            => v.CompareTo(min) <= 0 ? min : v;
        public static bool Between<T>(this T v, T from, T to) where T : IComparable
            => from.CompareTo(to) < 0 ? v.CompareTo(from) >= 0 && v.CompareTo(to) <= 0 :
                                        v.CompareTo(to) >= 0 && v.CompareTo(from) <= 0;
    }
}